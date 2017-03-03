using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace coreJson
{
    public class CompiledPropertyAccessor<T>
    {
        private Action<T, object> _setter;
        private Func<T, object> _getter;
        public bool IsList { get; private set; }        
        public Type GenericType { get; private set; }
        public bool IsEnum { get; private set; }

        public string Name { get; private set; }
        public CompiledPropertyAccessor(PropertyInfo property)
        {
            _setter = MakeSetter(property);
            _getter = MakeGetter(property);
            TypeInfo ti = property.PropertyType.GetTypeInfo();
            IsList = property.PropertyType != typeof(string) && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ti);
            

            Name = property.Name;
            GenericType = property.PropertyType;
            if (IsList)
            {
                if (ti.IsArray)
                    GenericType = ti.GetElementType();
                else                
                    GenericType = ti.GenericTypeArguments.FirstOrDefault();

                if(GenericType != null)
                    ti = GenericType.GetTypeInfo();
                
                    
            }
                        
            IsEnum = ti.IsEnum;
        }

        public object Get(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            return _getter(entity);
        }

        public void Set(T entity, object value)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _setter(entity, value);
        }

        private static Action<T, object> MakeSetter(PropertyInfo property)
        {
            ParameterExpression entityParameter = Expression.Parameter(typeof(T));
            ParameterExpression objectParameter = Expression.Parameter(typeof(Object));
            MemberExpression toProperty = Expression.Property(Expression.TypeAs(entityParameter, property.DeclaringType), property);
            UnaryExpression fromValue = Expression.Convert(objectParameter, property.PropertyType);
            BinaryExpression assignment = Expression.Assign(toProperty, fromValue);
            Expression<Action<T, object>> lambda = Expression.Lambda<Action<T, object>>(assignment, entityParameter, objectParameter);
            return lambda.Compile();
        }

        private static Func<T, object> MakeGetter(PropertyInfo property)
        {
            ParameterExpression entityParameter = Expression.Parameter(typeof(T));
            MemberExpression fromProperty = Expression.Property(Expression.TypeAs(entityParameter, property.DeclaringType), property);
            UnaryExpression convert = Expression.Convert(fromProperty, typeof(Object));
            Expression<Func<T, object>> lambda = Expression.Lambda<Func<T, object>>(convert, entityParameter);
            return lambda.Compile();
        }
    }
}
