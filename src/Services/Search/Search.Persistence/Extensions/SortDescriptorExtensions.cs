using Abstractions;
using OpenSearch.Client;

namespace Search.Persistence.Extensions;

public static class SortDescriptorExtensions
{
    public static SearchDescriptor<TEntity> SortBy<TEntity>(
        this SearchDescriptor<TEntity> search,
        string? sortBy) where TEntity : class
    {
        return search.Sort(sort => sort.SortBy(sortBy));
    }

    public static IPromise<IList<ISort>> SortBy<TEntity>(
        this SortDescriptor<TEntity> sort,
        string? sortBy) where TEntity : class
    {
        var sortDefinition = QueryableSortBy.ParseToKeySelector<TEntity>(sortBy);

        return sortDefinition.Desc
            ? sort.Descending(sortDefinition.KeySelector)
            : sort.Ascending(sortDefinition.KeySelector);
    }
}