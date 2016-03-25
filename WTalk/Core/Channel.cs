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
            headerData.Add("CI", "1");  // 0 if streaming/chunked requests should be used
            headerData.Add("AID", _aid);  // 
            headerData.Add("TYPE", "xmlhttp");  // type of request
            // zx ?
            headerData.Add("t", "1");  // trial


            HttpResponseMessage message = await _client.Execute(HangoutUri.CHANNEL_URL + "channel/bind", headerData);
            if (message != null)
            {
                message.EnsureSuccessStatusCode();
                dataReceived(await message.Content.ReadAsStringAsync());
            }

            #region if streaming 
            //StringBuilder query = new StringBuilder(HangoutUri.CHANNEL_URL + "channel/bind?");


            //query.Append(string.Join("&", headerData.Select(c => string.Format("{0}={1}", c.Key, Uri.EscapeUriString(c.Value))).ToArray()));


            //using (System.IO.StreamReader reader = new System.IO.StreamReader( await _client.GetStreamAsync(query.ToString()), true))
            //{
            //    int response_count =0, chunkSize= 0;
            //    char[] buffer;
            //    string data;                 
            //    try
            //    {

            //        while (!reader.EndOfStream && response_count < 5)
            //        {
            //            data = reader.ReadLine();
            //            if (data == null)
            //                break;


            //            chunkSize = int.Parse(data);
            //            buffer = new char[chunkSize];
            //            reader.Read(buffer, 0, chunkSize);

            //            dataReceived(new string(buffer));
            //            response_count++;                        
            //        }
            //    }
            //    catch
            //    {

            //    }





            //StringBuilder builder = new StringBuilder();
            //string data = reader.ReadLine();
            //int chunkSize = int.Parse(data);
            //char[] buffer;
            //int response_count = 0;

            //while (!reader.EndOfStream && response_count < 5)
            //{

            //    buffer = new char[chunkSize];
            //    reader.Read(buffer, 0, chunkSize);
            //    builder.Append(buffer);


            //    dataReceived(builder.ToString());
            //    response_count++;
            //    //reader.BaseStream.Position = chunkSize;
            //    builder.Clear();
            //    data = reader.ReadLine();
            //    if (data == null)
            //        break;
            //    chunkSize = int.Parse(data);
            //}

        //}
#endregion

        }

        private void dataReceived(string data)
        {

            _logger.Debug("Received data : {0}", data.Replace("\n", ""));

            Connected = true;

            // parse chunk data
            // chunk contains a container array
            // remove length information    

            string[] chunkDatas = Parser.CleanDataArray(data);
            JArray chunkJsonData = null;
            foreach (string chunkData in chunkDatas)
            {
                chunkJsonData = Parser.ParseData(chunkData);
                foreach (var chunkJson in chunkJsonData)
                {
                    // first part is aid
                    _aid = chunkJson[0].ToString();
                    if (chunkJson[1] != null && OnDataReceived != null)
                        OnDataReceived(this, chunkJson[1] as JArray);
                }
            }
        }

        /// <summary>
        /// Acknowledgement request, sent when client id changes and used to register to services
        /// </summary>
        /// <param name="lastSubscribe"></param>
        internal async void SendAck(long lastSubscribe)
        {
            TimeSpan epoch = DateTime.UtcNow.TimeIntervalSince1970();

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
            string response = await sendMapsRequest(subscribeData);
                        
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
            string response = await sendMapsRequest(new Dictionary<string, string>());            
            JArray array = Parser.ParseData(response);
            _sid = array[0][1][1].ToString();
            if(array.Count>1)
                _gsession_id = array[1][1][0]["gsid"].ToString();
        }

        /// <summary>
        /// Sends a request to the server containing maps (dicts).
        /// </summary>
        /// <returns></returns>
        async Task<string> sendMapsRequest(Dictionary<string, string> map_list = null)
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
                      

            HttpResponseMessage response = await _client.Execute(HangoutUri.CHANNEL_URL + "channel/bind", headerData, map_list);
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);            
            return await response.Content.ReadAsStringAsync();
        }

    }
}
