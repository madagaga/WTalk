using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace coreJson
{
    public class JSON
    {
        internal static ConcurrentDictionary<Type, Dictionary<string, CompiledPropertyAccessor<object>>> _propertyCache = new ConcurrentDictionary<Type, Dictionary<string,  CompiledPropertyAccessor<object>>>();
        internal static ConcurrentDictionary<Type, Delegate> _constructorCache = new ConcurrentDictionary<Type, Delegate>();

        internal static void initializeCache(Type type)
        {
            if (!_propertyCache.ContainsKey(type))
                _propertyCache.GetOrAdd(
                    type,
                    (backingType) => backingType
                        .GetTypeInfo().DeclaredProperties
                        //.Where(p => p.GetSetMethod() != null && p.GetGetMethod() != null)
                        .Select<PropertyInfo, CompiledPropertyAccessor<object>>(p => new CompiledPropertyAccessor<object>(p))
                        .ToDictionary(p => p.Name));

            if (!_constructorCache.ContainsKey(type))
                _constructorCache.GetOrAdd(
                    type,
                    Expression.Lambda(Expression.New(type)).Compile());
        }

        public static object Parse(System.IO.Stream input)
        {
            using (JsonParser parser = new JsonParser())
                return parser.Parse(input);
        }

        public static object Parse(string input)
        {
            using (JsonParser parser = new JsonParser())
                return parser.Parse(input);
        }
               

        public static T Deserialize<T>(string input) where T : new()
        {
            object o = null;
            using (JsonParser parser = new JsonParser())
                o = parser.Parse(input);
            
            if (o is IDictionary)
            {
                //if root element is not type
                if (((IDictionary)o).Contains(typeof(T).Name.ToLower()))
                {   
                    o = ((Dictionary<string, object>)o)[typeof(T).Name.ToLower()];
                }
                return (T)ParseObject(typeof(T), (Dictionary<string, object>)o);
            }
            else if( o is IList)
            {
                return (T)ParseArray(typeof(T), (List<object>)o);
            }
            return default(T);
        }


        private static object ParseObject(Type type, Dictionary<string, object> input)
        {
            if (input == null)
                return null;

            initializeCache(type);            

            object result = _constructorCache[type].DynamicInvoke(null);
            var properties = _propertyCache[type];
            foreach (var property in properties.Values)
            {
                if (!input.ContainsKey(property.Name))
                    continue;


                if (input[property.Name] is IDictionary)
                    property.Set(result, ParseObject(property.GenericType, (Dictionary<string, object>)input[property.Name]));
                else if (input[property.Name] is IList)
                    property.Set(result, ParseArray(property.GenericType, (List<object>)input[property.Name]));
                else if(property.IsEnum)
                {
                    if (input[property.Name] != null)
                        property.Set(result, Enum.Parse(property.GenericType, input[property.Name].ToString()));
                }                
                else
                    property.Set(result, Convert.ChangeType(input[property.Name], property.GenericType));
            }
            return result;
        }

        private static object ParseArray(Type type, List<object> list)
        {
            if (list == null)
                return null;

            var genericType = typeof(List<>).MakeGenericType(type);
            IList result = (IList)Expression.Lambda(Expression.New(genericType)).Compile().DynamicInvoke(null);
            foreach(object o in list)
            {
                if (o is IDictionary)
                    result.Add(ParseObject(type, (Dictionary<string, object>)o));
                else if(o is IList)
                    result.Add(ParseArray(type, (List<object>)o));
                else 
                    result.Add(o);
            }
            return result;
        }

        

        public static string Serialize<T>(T input) where T : new()
        {
            return new JsonSerializer().ConvertToJSON(input);            
        }      

       
    }
}
