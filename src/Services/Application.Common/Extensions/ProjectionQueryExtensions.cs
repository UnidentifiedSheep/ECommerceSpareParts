using Application.Common.Interfaces.Projections;
using LinqKit;

namespace Application.Common.Extensions;

public static class ProjectionQueryExtensions
{
    public static IQueryable<TOut> Project<TIn, TOut>(
        this IQueryable<TIn> query,
        IProjectionProvider<TIn, TOut> provider)
    {
        return query
            .AsExpandable()
            .Select(provider.Projection);
    }
}
