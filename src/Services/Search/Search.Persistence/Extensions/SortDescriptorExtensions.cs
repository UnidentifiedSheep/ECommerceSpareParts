using Abstractions;
using Exceptions;
using OpenSearch.Client;
using System.Linq.Expressions;

namespace Search.Persistence.Extensions;

public static class SortDescriptorExtensions
{
    public static SearchDescriptor<TEntity> SortBySearchRelevance<TEntity, TKey>(
        this SearchDescriptor<TEntity> search,
        string[]? sortBy,
        Expression<Func<TEntity, TKey>> idSelector)
        where TEntity : class
    {
        if (sortBy is not null && sortBy.Any(x => !string.IsNullOrWhiteSpace(x)))
            return search.SortBy(sortBy);

        return search.Sort(sort => sort
            .Descending(SortSpecialField.Score)
            .Ascending(idSelector));
    }

    public static SearchDescriptor<TEntity> SortBy<TEntity>(
        this SearchDescriptor<TEntity> search,
        string[]? sortBy,
        bool useDefault = true) where TEntity : class
    {
        if (!useDefault && (sortBy is null || sortBy.All(string.IsNullOrWhiteSpace)))
            return search;

        return search.Sort(sort => sort.SortBy(sortBy));
    }

    public static IPromise<IList<ISort>> SortBy<TEntity>(
        this SortDescriptor<TEntity> sort,
        string[]? sortBy) where TEntity : class
    {
        IReadOnlyList<global::Abstractions.Models.SortyBy.KeySelectorSortDefinition<TEntity>> definitions;
        try
        {
            definitions = QueryableSortBy.ParseToKeySelectors<TEntity>(sortBy);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidInputException(
                "sorting.invalid",
                [exception.Message],
                exception.Message);
        }

        foreach (var definition in definitions)
        {
            sort = definition.Desc
                ? sort.Descending(definition.KeySelector)
                : sort.Ascending(definition.KeySelector);
        }

        return sort;
    }
}
