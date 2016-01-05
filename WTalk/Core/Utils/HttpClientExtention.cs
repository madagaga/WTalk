using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WTalk.Core.ProtoJson;

namespace WTalk.Core.Utils
{
    public static class HttpClientExtention
    {

        static NLog.Logger _logger = NLog.LogManager.GetLogger("HttpClient");        

        public static HttpResponseMessage Execute(this HttpClient client, string url, Dictionary<string, string> queryString = null, Dictionary<string, string> postData = null)
        {
            StringBuilder query = new StringBuilder(url);

            if (queryString != null)
            {
                if (url.IndexOf("?") < 0)
                    query.Append("?");
                query.Append(string.Join("&",queryString.Select(c=>string.Format("{0}={1}", c.Key, Uri.EscapeUriString(c.Value))).ToArray()));                                
            }
            
            HttpResponseMessage message = null;
            if (postData != null)
                message = client.PostAsync(query.ToString(), new FormUrlEncodedContent(postData)).Result;
            else
                message = client.GetAsync(query.ToString()).Result;

            _logger.Debug("Received data : {0}", message.Content.ReadAsStringAsync().Result.Replace("\n", ""));

            return message;
        }

        public static HttpResponseMessage PostJson(this HttpClient client, string apiKey, string endPoint, JArray body)
        {
            return client.execute(apiKey, endPoint, body, true);
        }

        public static HttpResponseMessage PostProtoJson<T>(this HttpClient client, string apiKey, string endPoint, T protoJsonObject) where T:class
        {
            JArray body = ProtoJsonSerializer.Serialize(protoJsonObject);
            return client.execute(apiKey, endPoint, body, false);
        }

        static HttpResponseMessage execute(this HttpClient client, string apiKey, string endPoint, JArray body, bool useJson)
        {
            HttpResponseMessage message = null;
            _logger.Debug("Sending Request : {0}", endPoint);
            _logger.Debug("Sending data : {0}", body.ToString().Replace("\r\n", ""));
            string uri = string.Format("{0}{1}?key={2}&alt={3}",HangoutUri.CHAT_SERVER_URL,endPoint, Uri.EscapeUriString(apiKey), useJson ? "json" : "protojson");
            message = client.PostAsync(uri,new StringContent(body.ToString(),Encoding.UTF8,"application/json+protobuf")).Result;
            message.EnsureSuccessStatusCode();
            _logger.Debug("Received data : {0}", message.Content.ReadAsStringAsync().Result.Replace("\n", ""));
            return message;
        }

        public static T ReadAsProtoJson<T>(this HttpContent content) where T:new() 
        {
            JArray arrayBody = JArray.Parse(content.ReadAsStringAsync().Result);
            arrayBody.RemoveAt(0);
            return ProtoJsonSerializer.Deserialize<T>(arrayBody);
        }
    }
}
