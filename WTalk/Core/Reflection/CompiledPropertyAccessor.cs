using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace WTalk.Core.Reflection
{

    // dynamic 
    public class CompiledPropertyAccessor<T>
    {        
        private Action<T, object> _setter;
        private Func<T, object> _getter;
        
        public CompiledPropertyAccessor(PropertyInfo property)            
        {            
            _setter = MakeSetter(property);
            _getter = MakeGetter( property);
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
