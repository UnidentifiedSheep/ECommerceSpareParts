using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Application.Common.Extensions;

public static class ExpressionExtensions
{
    private static readonly ConcurrentDictionary<LambdaExpression, Delegate> Cache = new();

    public static Func<TSource, TDest> AsFunc<TSource, TDest>(
        this Expression<Func<TSource, TDest>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return (Func<TSource, TDest>)Cache.GetOrAdd(
            expression,
            expr => expr.Compile());
    }
}