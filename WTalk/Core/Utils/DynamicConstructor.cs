using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

namespace WTalk.Core.ProtoJson
{
    internal static class New
    {
        public static Delegate Creator(Type t)
        {            
            if (t == typeof(string))
                return Expression.Lambda(Expression.Constant(string.Empty)).Compile();

            if (t.HasDefaultConstructor())
                return Expression.Lambda(Expression.New(t)).Compile();

            return null;
        }
    }

    internal static class TypeExt
    {
        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}

/*
 * public static class New<T>
{
    public static readonly Func<T> Instance = Creator();

    static Func<T> Creator()
    {
        Type t = typeof(T);
        if (t == typeof(string))
            return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();

        if (t.HasDefaultConstructor())
            return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();

        return () => (T)FormatterServices.GetUninitializedObject(t);
    }
}

public static bool HasDefaultConstructor(this Type t)
{
    return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
}*/

