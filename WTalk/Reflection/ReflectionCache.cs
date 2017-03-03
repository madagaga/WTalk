using System;

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Core.Reflection
{
    public static class ReflectionCache
    {
        internal static Dictionary<Type, List<ProtoJsonPropertyAccessor>> _propertyCache = new Dictionary<Type, List<ProtoJsonPropertyAccessor>>();
        internal static Dictionary<Type, Delegate> _constructorCache = new Dictionary<Type, Delegate>();


        private static void initializePropertyCache(Type type)
        {
            if (!_propertyCache.ContainsKey(type))
                _propertyCache.Add(
                    type,
                    type.GetTypeInfo().DeclaredProperties.Select(p => new ProtoJsonPropertyAccessor(p)).ToList());

            if (!_constructorCache.ContainsKey(type))
                _constructorCache.Add(
                    type,
                    Expression.Lambda(Expression.New(type)).Compile());
        }

        internal static Delegate GetConstructor(Type type)
        {
            initializePropertyCache(type);
            return _constructorCache[type];
        }

        internal static List<ProtoJsonPropertyAccessor> GetProperties(Type type)
        {
            initializePropertyCache(type);
            return _propertyCache[type];
        }
    }
}
