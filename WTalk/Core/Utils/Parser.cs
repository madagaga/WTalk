using coreJson;
using System.Collections.Generic;

namespace WTalk.Core.Utils
{
    public static class Parser
    {
        static NLog.Logger _logger = NLog.LogManager.GetLogger("Parser");
        //static System.Text.RegularExpressions.Regex _cleanRegex = new System.Text.RegularExpressions.Regex(@"\d+\n");        

        public static List<DynamicJson> ParseInitParams(string data)
        {
            // manual parse ... regex doesn't work 
            string start_string = "<script>AF_initDataCallback(", end_string = ");</script>", extractedData = null;
            int start_length = start_string.Length, current_index = data.IndexOf(start_string), end_index = -1;
            DynamicJson parsedObject;
            List<DynamicJson> dataDictionary = new List<coreJson.DynamicJson>();

            while (current_index > 0)
            {
                end_index = data.IndexOf(end_string, current_index);
                extractedData = data.Substring(current_index + start_length, end_index - current_index - start_length);
                if (extractedData.IndexOf("data:function") > -1)
                    extractedData = extractedData.Replace("data:function(){return", "data:").Replace("}}", "}");
                parsedObject =new DynamicJson(extractedData);
                dataDictionary.Add(parsedObject["data"] );
                current_index = data.IndexOf(start_string, end_index);
            }

            return dataDictionary;
        }

    }
}
