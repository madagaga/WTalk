using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WTalk.ProtoJson
{
    public static class ProtoJsonSerializer
    {
        public static JArray Serialize<T>(T protojson) where T : class
        {   
            return ToJson(typeof(T), protojson);
        }

        private static JArray ToJson(Type type, object protojson)
        {
            JArray result = new JArray();            
            if (!type.IsDefined(typeof(ProtoContractAttribute), false))
                throw new Exception("Object does not have ProtoContractAttribute");
            else
            {
                var properties = type.GetProperties();
                ProtoMemberAttribute attribute;
                int previousIndex = 0;
                object currentValue;

                foreach (var property in properties)
                {
                    attribute = (ProtoMemberAttribute)property.GetCustomAttributes(typeof(ProtoMemberAttribute), false).First();

                    //for (int i = previousIndex; i < attribute.Position; i++)
                    //    result.Add(null);

                    currentValue = property.GetValue(protojson, null);
                    if (currentValue == null)
                        if (attribute.Optional)
                            continue;
                        else
                        {
                            result.Add((JToken)null);
                            continue;
                        }

                    if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        JArray list = new JArray();
                        Type genericType = property.PropertyType.GetGenericArguments().First();
                        foreach (var item in currentValue as IEnumerable)
                            if (genericType.IsDefined(typeof(ProtoContractAttribute), false))
                                list.Add(ToJson(genericType, item));
                            else
                                list.Add(item);

                        result.Add(list);
                    }
                    else if (property.PropertyType.IsDefined(typeof(ProtoContractAttribute), false))
                        result.Add(ToJson(property.PropertyType, currentValue));
                    else if (property.PropertyType.IsEnum)
                        result.Add((int)currentValue);                    
                    else
                        result.Add(currentValue);
                    previousIndex = attribute.Position;
                }


            }
            return result;
        }



        public static T Deserialize<T>(JArray json) where T:new()
        {
            return (T)ParseObject(typeof(T), json);
        }

        private static object ParseObject(Type type, JArray jArray)
        {
            if (jArray == null)
                return null;
            object result = New.Creator(type).DynamicInvoke(null);
            
            if (!type.IsDefined(typeof(ProtoContractAttribute), false))
                throw new Exception("Object does not have ProtoContractAttribute");
            else
            {
                var properties = type.GetProperties();
                ProtoMemberAttribute attribute;
                
                foreach (var property in properties)
                {
                    try
                    {
                        attribute = (ProtoMemberAttribute)property.GetCustomAttributes(typeof(ProtoMemberAttribute), false).First();
                        if (jArray.Count > attribute.Position && jArray[attribute.Position] != null)
                        {

                            if (property.PropertyType.IsDefined(typeof(ProtoContractAttribute), false))
                                property.SetValue(result, ParseObject(property.PropertyType, jArray[attribute.Position] as JArray), null);
                            else if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                            {
                                var list = New.Creator(property.PropertyType).DynamicInvoke(null);                                
                                foreach (var jtoken in jArray[attribute.Position])
                                    list.GetType().GetMethod("Add").Invoke(list, new[] { ParseObject(property.PropertyType.GetGenericArguments().First(), jtoken as JArray) });
                                property.SetValue(result, list, null);
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                if (!string.IsNullOrEmpty(jArray[attribute.Position].ToString()))
                                    property.SetValue(result, Enum.Parse(property.PropertyType, jArray[attribute.Position].ToString()), null);
                            }
                            else
                            {
                                if (property.PropertyType == typeof(string) || !string.IsNullOrEmpty(jArray[attribute.Position].ToString()))
                                    property.SetValue(result, Convert.ChangeType(jArray[attribute.Position], property.PropertyType), null);
                            }
                        }
                    }
                    catch { }

                }
            }

            return result;
        }




    }
}
