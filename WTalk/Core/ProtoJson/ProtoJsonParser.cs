using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using WTalk.Core.Reflection;
using coreJson;

namespace WTalk.Core.ProtoJson
{
    public static class ProtoJsonSerializer
    {
        

        public static List<object> Serialize<T>(T protojson) where T : class
        {   
            return ToJson(typeof(T), protojson);
        }

        private static List<object> ToJson(Type type, object protojson)
        {
            List<object> result = new List<object>();

            IEnumerable<ProtoJsonPropertyAccessor> properties = ReflectionCache.GetProperties(type);
            object currentValue;

            foreach (var property in properties)
            {
                currentValue = property.Get(protojson);
                if (currentValue == null)
                    if (property.IsOptional)
                        continue;
                    else
                    {
                        result.Add((object)null);
                        continue;
                    }

                if (property.IsList)
                {
                    List<object> list = new List<object>();
                    foreach (var item in currentValue as IEnumerable)
                    {
                        if (property.IsProtoContract)
                            list.Add(ToJson(property.UnderlyingType, item));
                        else if (property.IsEnum)
                            list.Add((int)item);
                        else
                            list.Add(item);
                    }

                    result.Add(list);
                }
                else if (property.IsProtoContract)
                    result.Add(ToJson(property.UnderlyingType, currentValue));
                else if (property.IsEnum)
                    result.Add((int)currentValue);
                else
                    result.Add(currentValue);
            }

            return result;
        }


        public static T Deserialize<T>(DynamicJson json) where T:new()
        {
            return (T)ParseObject(typeof(T), json);
        }

        private static object ParseObject(Type type, DynamicJson jArray)
        {
            if (jArray == null)
                return null;

            
            object result = ReflectionCache.GetConstructor(type).DynamicInvoke(null);
            var properties = ReflectionCache.GetProperties(type);

            foreach (var property in properties)
            {
                try
                {
                    if (jArray.Count > property.Position && jArray[property.Position] != null)
                    {
                        if (property.IsList/*&& property.IsProtoContract*/)
                        {                            
                            var genericType = typeof(List<>).MakeGenericType(property.UnderlyingType);
                            IList list = (IList)Expression.Lambda(Expression.New(genericType)).Compile().DynamicInvoke(null);

                            if (jArray[property.Position].Count > 0)
                            {
                                object listItem = null;
                                foreach (DynamicJson jtoken in jArray[property.Position])
                                {
                                    if (property.IsProtoContract)
                                        listItem = ParseObject(property.UnderlyingType, jtoken);
                                    else if (property.IsEnum)
                                        listItem = Enum.Parse(property.UnderlyingType, jtoken.ToString());
                                    else 
                                        listItem = jtoken.Value;

                                    if (listItem != null)
                                        list.Add(listItem);
                                }
                            }
                            property.Set(result, list);
                        }
                        else if (property.IsProtoContract)
                            property.Set(result, ParseObject(property.UnderlyingType, jArray[property.Position]));
                        else if (property.IsEnum)
                        {
                            if (!string.IsNullOrEmpty(jArray[property.Position].ToString()))
                                property.Set(result, Enum.Parse(property.UnderlyingType, jArray[property.Position].ToString()));
                        }
                        else
                        {
                            if (property.UnderlyingType == typeof(string) || !string.IsNullOrEmpty(jArray[property.Position].ToString()))
                                property.Set(result, Convert.ChangeType(jArray[property.Position].Value, property.UnderlyingType));
                        }
                    }
                }
                catch (Exception e) { }

            }


            return result;
        }




    }
}
