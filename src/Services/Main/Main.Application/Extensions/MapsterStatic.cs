using System.Linq.Expressions;
using Core.Models;
using Mapster;

namespace Main.Application.Extensions;

public static class MapsterStatic
{
    public static TypeAdapterSetter<TSource, TDestination> IgnorePatchIfNotSet<TSource, TDestination>(
        this TypeAdapterSetter<TSource, TDestination> config)
    {
        var sourceType = typeof(TSource);
        var destinationType = typeof(TDestination);

        var patchProps = sourceType.GetProperties()
            .Where(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(PatchField<>));

        foreach (var prop in patchProps)
        {
            var propName = prop.Name;
            var srcParam = Expression.Parameter(sourceType, "src");
            var destParam = Expression.Parameter(destinationType, "dest");
            var srcProp = Expression.Property(srcParam, prop);
            var isSetProp = Expression.Property(srcProp, "IsSet");
            var notIsSet = Expression.Not(isSetProp);
            var lambda = Expression.Lambda<Func<TSource, TDestination, bool>>(notIsSet, srcParam, destParam);
            var destProp = destinationType.GetProperty(propName);

            if (destProp == null) continue;

            var destParamExpr = Expression.Parameter(destinationType, "dest");
            var destPropExpr = Expression.Property(destParamExpr, destProp);
            var destPropCast = Expression.Convert(destPropExpr, typeof(object));
            var destLambda = Expression.Lambda<Func<TDestination, object>>(destPropCast, destParamExpr);
            config = config.IgnoreIf(lambda, destLambda);
        }

        return config;
    }
}