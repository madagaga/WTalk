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
                    // TODO : extract and recursive 
                    List<object> list = new List<object>();
                    foreach (var item in currentValue as IEnumerable)
                    {
                        switch (property.ProtoJsonType)
                        {
                            case AttributeTypeEnum.ProtoContract:
                                list.Add(ToJson(property.GenericType, item));
                                break;
                            case AttributeTypeEnum.Enum:
                                list.Add((int)item);
                                break;
                            case AttributeTypeEnum.Value:
                                list.Add(item);
                                break;
                            default:
                                throw new Exception("should not be here");

                        }
                    }
                    result.Add(list);
                }
                else
                {
                    switch (property.ProtoJsonType)
                    {
                        case AttributeTypeEnum.ProtoContract:
                            result.Add(ToJson(property.GenericType, currentValue));
                            break;
                        case AttributeTypeEnum.Enum:
                            result.Add((int)currentValue);
                            break;
                        case AttributeTypeEnum.Value:
                            result.Add(currentValue);
                            break;
                        default:
                            throw new Exception("should not be here");
                    }
                }


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
                    if (jArray.Count < property.Position || jArray[property.Position] == null)
                        continue;


                    if (property.IsList)
                    {
                        var genericType = typeof(List<>).MakeGenericType(property.GenericType);
                        IList list = (IList)Expression.Lambda(Expression.New(genericType)).Compile().DynamicInvoke(null);

                        if (jArray[property.Position].Count > 0)
                        {
                            object listItem = null;
                            foreach (DynamicJson jtoken in jArray[property.Position])
                            {
                                switch (property.ProtoJsonType)
                                {
                                    case AttributeTypeEnum.ProtoContract:
                                        listItem = ParseObject(property.GenericType, jtoken);
                                        break;
                                    case AttributeTypeEnum.Enum:
                                        listItem = Enum.Parse(property.GenericType, jtoken.ToString());
                                        break;
                                    default:
                                        listItem = jtoken.Value;                                       
                                        break;
                                }
                                if (listItem != null)
                                    list.Add(listItem);
                            }
                        }
                        property.Set(result, list);
                    }
                    else
                    {
                        switch (property.ProtoJsonType)
                        {
                            case AttributeTypeEnum.ProtoContract:
                                property.Set(result, ParseObject(property.GenericType, jArray[property.Position]));
                                break;
                            case AttributeTypeEnum.Enum:
                                if (!string.IsNullOrEmpty(jArray[property.Position].ToString()))
                                    property.Set(result, Enum.Parse(property.GenericType, jArray[property.Position].ToString()));
                                break;
                            default:
                                if (property.GenericType.IsPrimitiveType() || !string.IsNullOrEmpty(jArray[property.Position].ToString()))
                                    property.Set(result, Convert.ChangeType(jArray[property.Position].Value, property.GenericType));
                                break;
                        }
                    }
                }
                catch (Exception e) { }

            }


            return result;
        }




    }
}
