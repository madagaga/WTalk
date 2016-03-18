using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Model;

namespace WTalk.Core.Utils
{
    public static class Parser
    {
        static NLog.Logger _logger = NLog.LogManager.GetLogger("Parser");
        static System.Text.RegularExpressions.Regex _cleanRegex = new System.Text.RegularExpressions.Regex(@"\d+\n");        

        public static string CleanData(string data)
        {
            return string.Join(",", CleanDataArray(data));
        }

        public static string[] CleanDataArray(string data)
        {
            string[] splitted = _cleanRegex.Split(data);
            return splitted.Where(c => !string.IsNullOrEmpty(c)).ToArray();
        }

        public static JArray ParseData(string data)
        {
            return JArray.Parse(CleanData(data));
        }

        public static IEnumerable<JArray> ParseChunkData(string data)
        {
            // chunk contains a container array
            string[] chunks = CleanDataArray(data);

            foreach (string chunk in chunks)
                yield return JArray.Parse(chunk)[0][1] as JArray;
        }

        public static Dictionary<string, Newtonsoft.Json.Linq.JArray> ParseInitParams(string data)
        {
            // manual parse ... regex doesn't work 
            string start_string = "<script>AF_initDataCallback(";
            string end_string = ");</script>";
            string extractedData;
            int current_index = data.IndexOf(start_string), end_index = -1;
            Newtonsoft.Json.Linq.JObject parsedObject;
            Dictionary<string, Newtonsoft.Json.Linq.JArray> dataDictionary = new Dictionary<string, Newtonsoft.Json.Linq.JArray>();
            
            while (current_index > 0)
            {
                end_index = data.IndexOf(end_string, current_index);
                extractedData = data.Substring(current_index + start_string.Length, end_index - current_index - start_string.Length);
                if (extractedData.IndexOf("data:function") > -1)
                    extractedData = extractedData.Replace("data:function(){return", "data:").Replace("}}", "}");
                parsedObject = Newtonsoft.Json.Linq.JObject.Parse(extractedData);
                dataDictionary.Add(parsedObject["key"].ToString(), parsedObject["data"] as Newtonsoft.Json.Linq.JArray);
                current_index = data.IndexOf(start_string, end_index);
            }

            return dataDictionary;
        }

    }
}
