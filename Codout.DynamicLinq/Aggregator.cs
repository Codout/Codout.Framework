using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Codout.DynamicLinq;

/// <summary>
///     Represents a aggregate expression of Kendo DataSource.
/// </summary>
[DataContract(Name = "aggregate")]
public class Aggregator
{
    /// <summary>
    ///     Gets or sets the name of the aggregated field (property).
    /// </summary>
    [DataMember(Name = "field")]
    public string Field { get; set; }

    /// <summary>
    ///     Gets or sets the aggregate.
    /// </summary>
    [DataMember(Name = "aggregate")]
    public string Aggregate { get; set; }

    /// <summary>
    ///     Get MethodInfo.
    /// </summary>
    /// <param name="type">Specifies the type of querable data.</param>
    /// <returns>A MethodInfo for field.</returns>
    public MethodInfo MethodInfo(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type), "Type cannot be null.");

        var propType = type.GetProperty(Field)?.PropertyType;

        if (propType == null)
            throw new ArgumentException($"Property '{Field}' not found in type '{type.FullName}'.");

        switch (Aggregate)
        {
            case "max":
            case "min":
                return GetMethod(ConvertTitleCase(Aggregate), MinMaxFunc().GetMethodInfo(), 2)
                    .MakeGenericMethod(type, propType);
            case "average":
            case "sum":
                return GetMethod(ConvertTitleCase(Aggregate),
                    ((Func<Type, Type[]>)GetType().GetMethod("SumAvgFunc", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(propType).Invoke(null, null))
                    .GetMethodInfo(), 1).MakeGenericMethod(type);
            case "count":
                return GetMethod(ConvertTitleCase(Aggregate),
                    Nullable.GetUnderlyingType(propType) != null
                        ? CountNullableFunc().GetMethodInfo()
                        : CountFunc().GetMethodInfo(), 1).MakeGenericMethod(type);
        }

        return null;
    }

    private static string ConvertTitleCase(string str)
    {
        var tokens = str.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            tokens[i] = token.Substring(0, 1).ToUpper() + token.Substring(1);
        }

        return string.Join(" ", tokens);
    }

    private static MethodInfo GetMethod(string methodName, MethodInfo methodTypes, int genericArgumentsCount)
    {
        var methods = from method in typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                      let parameters = method.GetParameters()
                      let genericArguments = method.GetGenericArguments()
                      where method.Name == methodName &&
                            genericArguments.Length == genericArgumentsCount &&
                            parameters.Select(p => p.ParameterType)
                                .SequenceEqual((Type[])methodTypes.Invoke(null, genericArguments))
                      select method;
        return methods.FirstOrDefault();
    }

    private static Func<Type, Type[]> CountNullableFunc()
    {
        return CountNullableDelegate;
    }

    private static Type[] CountNullableDelegate(Type t)
    {
        return
        [
            typeof(IQueryable<>).MakeGenericType(t),
            typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(t, typeof(bool)))
        ];
    }

    private static Func<Type, Type[]> CountFunc()
    {
        return CountDelegate;
    }

    private static Type[] CountDelegate(Type t)
    {
        return
        [
            typeof(IQueryable<>).MakeGenericType(t)
        ];
    }

    private static Func<Type, Type, Type[]> MinMaxFunc()
    {
        return MinMaxDelegate;
    }

    private static Type[] MinMaxDelegate(Type a, Type b)
    {
        return
        [
            typeof(IQueryable<>).MakeGenericType(a),
            typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(a, b))
        ];
    }

    private static Func<Type, Type[]> SumAvgFunc<TU>()
    {
        return SumAvgDelegate<TU>;
    }

    private static Type[] SumAvgDelegate<TU>(Type t)
    {
        return
        [
            typeof(IQueryable<>).MakeGenericType(t),
            typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(t, typeof(TU)))
        ];
    }
}