﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using coreJson;
using WTalk.Core.HttpHandler;
using WTalk.Core.Utils;

namespace WTalk
{
    internal class Channel
    {
        NLog.Logger _logger = NLog.LogManager.GetLogger("Channel");
        
        const int MAX_RETRIES = 5;
        HttpClient _client;       

        #region events        
        public event EventHandler<DynamicJson> OnDataReceived;
        #endregion
        
        string _sid, _gsession_id,_aid = "0";
        string _appver = "chat_frontend_20151111.11_p0"; // default 

        int ofs_count = 0;

        public bool Connected { get; private set; }

        public Channel()
        {
            _client = new HttpClient(new SigningMessageHandler());
            _client.Timeout = new TimeSpan(0, 0, 30);

        }

        public void setAppVer(string appver)
        {
            _appver = appver;
        }

        public async Task Listen()
        {
            int retries = MAX_RETRIES;            

            while (retries >= 0)
            {
                if (retries + 1 < MAX_RETRIES)
                {
                    int backoff_seconds = 2 * (MAX_RETRIES - retries);
                    _logger.Info("Backing off for {0} seconds", backoff_seconds);

                    await Task.Delay(backoff_seconds * 1000);
                }

                if (string.IsNullOrEmpty(_sid))
                {
                    await retrieve_sid();
                    if (!Connected)
                        SendAck(0);
                }

                try
                {
                    _logger.Info("Opening new long-polling request ({0})", _aid); 
                    await LongPollRequest();
                    retries = MAX_RETRIES;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("SID"))
                        _sid = _gsession_id = null;                     

                    _logger.Error("Long polling Request failed {0}", e);
                    retries -= 1;
                    if (Connected)
                        Connected = false;

                }

            }

            _logger.Error("Ran out of retries for long-polling request ");
        }

        /// <summary>
        /// Open a long-polling request and receive arrays.
        ///This method uses keep-alive to make re-opening the request faster, but
        ///the remote server will set the "Connection: close" header once an hour.
        ///Raises hangups.NetworkError or UnknownSIDError.
        /// </summary>
        /// <returns></returns>
        async Task LongPollRequest(bool streaming = true)
        {


            Dictionary<string, string> headerData = new Dictionary<string, string>()
            {
                { "ctype", "hangouts"},  // client type
                {"prop", "StartPage"},
                {"appver", _appver},  // client type
                {"gsessionid", _gsession_id},
                {"VER", "8"},  // channel protocol version
                {"RID", "rpc"},  // request identifier
                {"SID", _sid},  // session ID
                {"CI", "0"},  // 0 if streaming/chunked requests should be used
                {"AID", _aid},  // 
                {"TYPE", "xmlhttp"},  // type of request
                // zx ?
                {"t", "1"}  // trial
            };
           
            #region if streaming
            if (streaming)
            {   
                using (HttpResponseMessage message = await _client.Execute(HangoutUri.CHANNEL_URL + "channel/bind", headerData,null,HttpCompletionOption.ResponseHeadersRead))
                {
                    long last_sync_date = DateTime.UtcNow.ToUnixTime();
                    try
                    {
                        using(System.IO.Stream reader = await message.Content.ReadAsStreamAsync())
                        while (reader.CanRead)
                            dataReceived(await (DecodeStream(reader)));
                    }
                    catch (Exception e) {
                        if (message.ReasonPhrase.Contains("SID"))
                            throw new Exception(message.ReasonPhrase);
                        if(e.Message.Contains("Decode"))
                            dataReceived(new DynamicJson(string.Format("[[{0},[\"resync\", {1}]]]", (int.Parse(_aid) + 1), last_sync_date)));
                    }
                }

            }
            else
            {
                headerData["CI"] = "1";
                using (HttpResponseMessage message = await _client.Execute(HangoutUri.CHANNEL_URL + "channel/bind", headerData))
                {
                    if (message != null)
                    {
                        message.EnsureSuccessStatusCode();
                        dataReceived(await (DecodeStream(message.Content.ReadAsStreamAsync())));
                    }
                }
            }

            #endregion

        }

        public int GetSizeDescriptor(System.IO.Stream stream)
        {
            // search for the first new line
            byte[] buffer = new byte[1];
            List<char> content = new List<char>();

            int result = 0;

            while (buffer[0] != 10 && stream.CanRead)
            {                
                content.Add((char)buffer[0]);
                stream.Read(buffer, 0, 1);
            }

            int.TryParse(new string(content.Where(c => char.IsDigit(c)).ToArray()), out result);
            return result;
        }

        private void dataReceived(DynamicJson data)
        {
            Connected = true;
            if (data == null)
                return;
            foreach (DynamicJson chunkJson in data)
            {                
                if (chunkJson[1] != null && OnDataReceived != null)
                    OnDataReceived(this, chunkJson[1]);

                // first part is aid
                _aid = chunkJson[0].ToString();
            }
            
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        /// <summary>
        /// Acknowledgement request, sent when client id changes and used to register to services
        /// </summary>
        /// <param name="lastSubscribe"></param>
        internal async void SendAck(long lastSubscribe)
        {

            Dictionary<string, string> subscribeData = new Dictionary<string, string>()
            {
                {"count", "1"},
                { "ofs", (++ofs_count).ToString()},
                // tdryer version
                { "req0_p", "{\"3\": {\"1\": {\"1\": \"babel\"}}}"}

            };
            
            
            await sendMapsRequest(subscribeData);
                        
        }

        async Task retrieve_sid()
        {


            _logger.Info("Sending sid request");

            DynamicJson array = await sendMapsRequest(new Dictionary<string, string>());  
            _sid = array[0][1][1].ToString();
            if(array.Count>1)
                _gsession_id = array[1][1][0]["gsid"].ToString();
        }

        /// <summary>
        /// Sends a request to the server containing maps (dicts).
        /// </summary>
        /// <returns></returns>
        async Task<DynamicJson> sendMapsRequest(Dictionary<string, string> map_list = null)
        {
            Dictionary<string, string> headerData = new Dictionary<string, string>()
            {
                {"ctype", "hangouts"}, // client type
                {"prop", "StartPage"},
                {"appver", _appver}, // client type
                {"VER", "8"},  // channel protocol version
                {"CVER", "5"},  // ??   
                {"RID", "43117"},  // request identifier

                {"t", "1"}  // trial
            };

            
            if(!string.IsNullOrEmpty(_gsession_id))
                headerData.Add("gsessionid", _gsession_id);
            
            if (!string.IsNullOrEmpty(_sid))
                headerData.Add("SID", _sid);  // session ID
            
            using (HttpResponseMessage response = await _client.Execute(HangoutUri.CHANNEL_URL + "channel/bind", headerData, map_list))
            {
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);
                return await DecodeStream( response.Content.ReadAsStreamAsync());
            }
        }


        async Task<DynamicJson> DecodeStream(Task<System.IO.Stream> stream)
        {
            return await DecodeStream(await stream);
        }

        async Task<DynamicJson> DecodeStream(System.IO.Stream stream)
        {
            int expectedLength = 0, receivedLength = 1, readLength = 0;
            byte[] buffer = null;            
            try
            {
                while (stream.CanRead)
                {
                    if (expectedLength == 0)
                    {
                        expectedLength = GetSizeDescriptor(stream);
                        if (expectedLength == 0)
                            continue;
                        readLength = receivedLength = 0;
                        buffer = new byte[expectedLength];
                    }

                    readLength = await stream.ReadAsync(buffer, Math.Max(readLength - 1, 0), expectedLength - receivedLength);
                    receivedLength += readLength;

                    if (receivedLength == expectedLength)
                    {
                        string received = new string(buffer.Select(c => (char)c).ToArray());
                        _logger.Info("Received data : {0}", received);
                        return new DynamicJson(received);                        
                    }
                        
                }
            }
            catch (System.IO.IOException) { }                
            catch(Exception e)
            {                
                throw new Exception("Decode stream error");
            }

            return null;
        }
        
    }
}
