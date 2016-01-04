using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using WTalk.Core.Reflection;

namespace WTalk.Core.ProtoJson
{
    public static class ProtoJsonSerializer
    {
        internal static ConcurrentDictionary<Type, Dictionary<string, ProtoJsonPropertyAccessor>> _propertyCache = new ConcurrentDictionary<Type, Dictionary<string, ProtoJsonPropertyAccessor>>();
        private static void initializePropertyCache(Type type)
        {
            if (!_propertyCache.ContainsKey(type))
                _propertyCache.GetOrAdd(
                    type,
                    (backingType) => backingType
                        .GetTypeInfo().DeclaredProperties
                        //.Where(p => p.GetSetMethod() != null && p.GetGetMethod() != null)
                        .Select<PropertyInfo, ProtoJsonPropertyAccessor>(p => new ProtoJsonPropertyAccessor(p))
                        .ToDictionary(p => p.Property.Name));
        }

        public static JArray Serialize<T>(T protojson) where T : class
        {   
            return ToJson(typeof(T), protojson);
        }

        private static JArray ToJson(Type type, object protojson)
        {
            initializePropertyCache(type);

            JArray result = new JArray();

            Dictionary<string, ProtoJsonPropertyAccessor> properties = _propertyCache[type];
                object currentValue;

                foreach (var property in properties.Values)
                {   
                    currentValue = property.Get(protojson);
                    if (currentValue == null)
                        if (property.IsOptional)
                            continue;
                        else
                        {
                            result.Add((JToken)null);
                            continue;
                        }

                    if (property.IsList)
                    {
                        JArray list = new JArray();                        
                        foreach (var item in currentValue as IEnumerable)
                            if (property.IsProtoContract)
                                list.Add(ToJson(property.UnderlyingType, item));
                            else
                                list.Add(item);

                        result.Add(list);
                    }
                    else if (property.IsProtoContract)
                        result.Add(ToJson(property.Property.PropertyType, currentValue));
                    else if (property.IsEnum)
                        result.Add((int)currentValue);                    
                    else
                        result.Add(currentValue);
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

            initializePropertyCache(type);
            object result = Expression.Lambda(Expression.New(type)).Compile().DynamicInvoke(null);
            var properties = _propertyCache[type];

            foreach (var property in properties.Values)
            {
                try
                {
                    if (jArray.Count > property.Position && jArray[property.Position] != null)
                    {
                        if (property.IsList)
                        {
                            var genericType = typeof(List<>).MakeGenericType(property.UnderlyingType);
                            IList list = (IList)Expression.Lambda(Expression.New(genericType)).Compile().DynamicInvoke(null);
                            foreach (var jtoken in jArray[property.Position])
                                list.Add(ParseObject(property.UnderlyingType, jtoken as JArray));
                            property.Set(result, list);
                        }
                        else if (property.IsProtoContract)
                            property.Set(result, ParseObject(property.Property.PropertyType, jArray[property.Position] as JArray));
                        else if (property.IsEnum)
                        {
                            if (!string.IsNullOrEmpty(jArray[property.Position].ToString()))
                                property.Set(result, Enum.Parse(property.Property.PropertyType, jArray[property.Position].ToString()));
                        }
                        else
                        {
                            if (property.Property.PropertyType == typeof(string) || !string.IsNullOrEmpty(jArray[property.Position].ToString()))
                                property.Set(result, Convert.ChangeType(jArray[property.Position], property.Property.PropertyType));
                        }
                    }
                }
                catch { }

            }


            return result;
        }




    }
}
