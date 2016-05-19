using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WTalk.Core.HttpHandler;
using WTalk.Core.Utils;

namespace WTalk
{
    public class Channel
    {
        NLog.Logger _logger = NLog.LogManager.GetLogger("Channel");
        
        const int MAX_RETRIES = 5;
        HttpClient _client;       

        #region events        
        public event EventHandler<JArray> OnDataReceived;
        #endregion

        public bool Connected { get; private set; }

        string _sid, _gsession_id,_aid = "0";
        string _appver = "chat_frontend_20151111.11_p0"; // default 

        int ofs_count = 0;        
        //bool _initialized = false;


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
                    await retrieve_sid();

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
        async Task LongPollRequest()
        {
           

            Dictionary<string, string> headerData = new Dictionary<string, string>();
           
            headerData.Add("ctype", "hangouts");  // client type
            headerData.Add("prop", "ChromeApp");
            headerData.Add("appver", _appver);  // client type
            headerData.Add("gsessionid", _gsession_id);
            headerData.Add("VER", "8");  // channel protocol version
            headerData.Add("RID", "rpc");  // request identifier
            headerData.Add("SID", _sid);  // session ID
            headerData.Add("CI", "0");  // 0 if streaming/chunked requests should be used
            headerData.Add("AID", _aid);  // 
            headerData.Add("TYPE", "xmlhttp");  // type of request
            // zx ?
            headerData.Add("t", "1");  // trial


            //using (HttpResponseMessage message = await _client.Execute(HangoutUri.CHANNEL_URL + "channel/bind", headerData))
            //{
            //    if (message != null)
            //    {
            //        message.EnsureSuccessStatusCode();
            //        dataReceived(await message.Content.ReadAsStringAsync());
            //    }
            //}

            #region if streaming 
            StringBuilder query = new StringBuilder(HangoutUri.CHANNEL_URL + "channel/bind?");


            query.Append(string.Join("&", headerData.Select(c => string.Format("{0}={1}", c.Key, Uri.EscapeUriString(c.Value))).ToArray()));


            using (System.IO.Stream reader = await _client.GetStreamAsync(query.ToString()))
            {
                try
                {
                    while (reader.CanRead)
                        dataReceived(await (DecodeStream(reader)));
                }
                catch { }
            }



            #endregion

        }

        public int GetSizeDescriptor(System.IO.Stream stream)
        {
            // search for the first new line
            byte[] buffer = new byte[1];
            List<char> content = new List<char>();
            while (buffer[0] != '\n' && stream.CanRead)
            {                
                content.Add((char)buffer[0]);
                stream.Read(buffer, 0, 1);
            }

            return int.Parse(new string(content.Where(c=>char.IsDigit(c)).ToArray()));
        }

        private void dataReceived(JArray data)
        {
            Connected = true;
            foreach (var chunkJson in data)
            {
                // first part is aid
                _aid = chunkJson[0].ToString();
                if (chunkJson[1] != null && OnDataReceived != null)
                    OnDataReceived(this, chunkJson[1] as JArray);
            }

        }

        /// <summary>
        /// Acknowledgement request, sent when client id changes and used to register to services
        /// </summary>
        /// <param name="lastSubscribe"></param>
        internal async void SendAck(long lastSubscribe)
        {
            
            Dictionary<string, string> subscribeData = new Dictionary<string, string>();
            subscribeData.Add("count", "1");
            subscribeData.Add("ofs", (++ofs_count).ToString());

            // original data 
            //subscribeData.Add("req0_p", "{\"1\":{\"1\":{\"1\":{\"1\":3,\"2\":2}},\"2\":{\"1\":{\"1\":3,\"2\":2},\"2\":\"6.3\",\"3\":\"JS\",\"4\":\"lcsclient\"},\"3\":" + epoch.TotalMilliseconds.ToString("N0") + ",\"4\":" + lastSubscribe + ",\"5\":\"c1\"},\"3\":{\"1\":{\"1\":\"tango_service\"}}}");
            //subscribeData.Add("req1_p", "{\"1\":{\"1\":{\"1\":{\"1\":3,\"2\":2}},\"2\":{\"1\":{\"1\":3,\"2\":2},\"2\":\"6.3\",\"3\":\"JS\",\"4\":\"lcsclient\"},\"3\":" + epoch.TotalMilliseconds.ToString("N0") + ",\"4\":" + lastSubscribe + ",\"5\":\"c2\"},\"3\":{\"1\":{\"1\":\"babel\"}}}");
            //subscribeData.Add("req2_p", "{\"1\":{\"1\":{\"1\":{\"1\":3,\"2\":2}},\"2\":{\"1\":{\"1\":3,\"2\":2},\"2\":\"6.3\",\"3\":\"JS\",\"4\":\"lcsclient\"},\"3\":" + epoch.TotalMilliseconds.ToString("N0") + ",\"4\":" + lastSubscribe + ",\"5\":\"c3\"},\"3\":{\"1\":{\"1\":\"hangout_invite\"}}}");
                        
            // tdryer version
            subscribeData.Add("req0_p", "{\"3\": {\"1\": {\"1\": \"babel\"}}}");

            System.Threading.Tasks.Task.Delay(1000).Wait();           
            await sendMapsRequest(subscribeData);
                        
        }

        //// not used but this is initialization process
        //void initialize()
        //{
        //    Dictionary<string, string> headerData = new Dictionary<string, string>();
        //    headerData.Add("ctype", "hangouts");  // client type
        //    headerData.Add("appver", "wtalk");  // client type
        //    headerData.Add("VER", "8");  // channel protocol version            
        //    headerData.Add("t", "1");  // trial

        //    // first get gsessionid 
        //    string message;
        //    JArray array;
            
        //    HttpResponseMessage response = _client.Execute(HangoutUri.CHANNEL_URL + "gsid", null, null);
        //    response.EnsureSuccessStatusCode();
        //    message = response.Content.ReadAsStringAsync().Result;
        //    array = Parser.ParseData(message);
        //    _gsession_id = array[1].ToString();

        //    // set mode init
        //    headerData.Add("gsessionid", _gsession_id);
        //    headerData.Add("MODE", "init");
        //    response = _client.Execute(HangoutUri.CHANNEL_URL + "channel/cbp", headerData, null);
        //    response.EnsureSuccessStatusCode();
        //    message = response.Content.ReadAsStringAsync().Result;

        //    headerData.Remove("MODE");
        //    headerData.Add("TYPE", "xmlhttp");
        //    response = _client.Execute(HangoutUri.CHANNEL_URL + "channel/cbp", headerData, null);
        //    response.EnsureSuccessStatusCode();
        //    message = response.Content.ReadAsStringAsync().Result;

        //    _initialized = true;
        //}
        
        async Task retrieve_sid()
        {


            _logger.Info("Sending sid request");
                   
            JArray array = await sendMapsRequest(new Dictionary<string, string>());  
            _sid = array[0][1][1].ToString();
            if(array.Count>1)
                _gsession_id = array[1][1][0]["gsid"].ToString();
        }

        /// <summary>
        /// Sends a request to the server containing maps (dicts).
        /// </summary>
        /// <returns></returns>
        async Task<JArray> sendMapsRequest(Dictionary<string, string> map_list = null)
        {
            Dictionary<string, string> headerData = new Dictionary<string, string>();
            //            parameters.add(CHANNEL_URL, "channel/bind?");

            headerData.Add("ctype", "hangouts");  // client type
            headerData.Add("prop", "ChromeApp");
            headerData.Add("appver", _appver);  // client type
            if(!string.IsNullOrEmpty(_gsession_id))
                headerData.Add("gsessionid", _gsession_id);
            headerData.Add("VER", "8");  // channel protocol version
            headerData.Add("RID", "81188");  // request identifier
            if (!string.IsNullOrEmpty(_sid))
                headerData.Add("SID", _sid);  // session ID
                        
            headerData.Add("t", "1");  // trial


            using (HttpResponseMessage response = await _client.Execute(HangoutUri.CHANNEL_URL + "channel/bind", headerData, map_list))
            {
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);
                return await DecodeStream( response.Content.ReadAsStreamAsync());
            }
        }


        async Task<JArray> DecodeStream(Task<System.IO.Stream> stream)
        {
            return await DecodeStream(await stream);
        }

        async Task<JArray> DecodeStream(System.IO.Stream stream)
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
                        readLength = receivedLength = 0;
                        buffer = new byte[expectedLength];
                    }

                    readLength = await stream.ReadAsync(buffer, Math.Max(readLength - 1, 0), expectedLength - receivedLength);
                    receivedLength += readLength;

                    if (receivedLength == expectedLength)
                    {
                        string received = validateData(buffer);
                        _logger.Info("Received data : {0}", received);
                        return JArray.Parse(received);                        
                    }
                        
                }
            }
            catch
            {
                throw new Exception("Decode stream error");
            }

            return new JArray();
        }

        string validateData(byte[] data)
        {
            StringBuilder builder = new StringBuilder();

            int aCount = 0, bCount=0;

            for(int i= 0 ;i<data.Length;i++)
            {
                switch(data[i])
                {
                    case 123:
                        aCount ++;
                        break;
                    case 125:
                        aCount --;
                        break;
                    //case 92:
                    //    if (data[i - 1] == 92)
                    //        continue;
                    //    break;
                    //case '[':
                    //    bCount ++;
                    //    break;
                    //case ']':
                    //    bCount --;
                    //    break;
                }
                builder.Append((char)data[i]);
            }

            if (aCount > 0)
                builder.Append(new String('}', aCount));

            return builder.ToString();
        }

    }
}
