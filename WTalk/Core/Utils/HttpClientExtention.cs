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

        public static async Task<HttpResponseMessage> Execute(this HttpClient client, string url, Dictionary<string, string> queryString = null, Dictionary<string, string> postData = null)
        {            
            try
            {
                StringBuilder query = new StringBuilder(url);

                if (queryString != null)
                {
                    if (url.IndexOf("?") < 0)
                        query.Append("?");
                    query.Append(string.Join("&", queryString.Select(c => string.Format("{0}={1}", c.Key, Uri.EscapeUriString(c.Value))).ToArray()));
                }

               
                if (postData != null)
                    return await client.PostAsync(query.ToString(), new FormUrlEncodedContent(postData));
                else
                    return await client.GetAsync(query.ToString());
            }
            catch
            {

            }
            return null;
        }

        public async static Task<HttpResponseMessage> PostJson(this HttpClient client, string apiKey, string endPoint, JArray body)
        {
            return await client.execute(endPoint,apiKey, body, true);
        }

        public async static Task<HttpResponseMessage> PostProtoJson<T>(this HttpClient client, string endPoint, string apiKey, T protoJsonObject) where T : class
        {
            JArray body = ProtoJsonSerializer.Serialize(protoJsonObject);
            return await client.execute(endPoint,apiKey, body, false);
        }

        async static Task<HttpResponseMessage> execute(this HttpClient client, string endPoint, string apiKey, JArray body, bool useJson)
        {
             HttpResponseMessage message = null;
            try
            {
               
                _logger.Debug("Sending Request : {0}", endPoint);
               // _logger.Debug("Sending data : {0}", body.ToString().Replace("\r\n", ""));
//                string uri = string.Format("{0}{1}?alt={2}", HangoutUri.CHAT_SERVER_URL, endPoint, useJson ? "json" : "protojson");
                string uri = string.Format("{0}{1}?key={2}&alt={3}", HangoutUri.CHAT_SERVER_URL, endPoint, Uri.EscapeUriString(apiKey), useJson ? "json" : "protojson");
                message = await client.PostAsync(uri, new StringContent(body.ToString(), Encoding.UTF8, "application/json+protobuf"));
                _logger.Debug("Request : {0} => {1} ", endPoint, message.StatusCode);
                message.EnsureSuccessStatusCode();
               // _logger.Debug("Received data : {0}", message.Content.ReadAsStringAsync().Result.Replace("\n", ""));
                
            }
            catch
            {

            }
            return message;
        }

        public async static Task<T> ReadAsProtoJson<T>(this HttpContent content) where T : new()
        {
            using (System.IO.Stream stream = await content.ReadAsStreamAsync())
            {
                var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StreamReader(stream));
                var arrayBody = JArray.Load(reader);
                arrayBody.RemoveAt(0);
                return ProtoJsonSerializer.Deserialize<T>(arrayBody);
            }
        }
    }
}
