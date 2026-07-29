using Abstractions;
using Exceptions;
using OpenSearch.Client;

namespace Search.Persistence.Extensions;

public static class SortDescriptorExtensions
{
    public static SearchDescriptor<TEntity> SortBy<TEntity>(
        this SearchDescriptor<TEntity> search,
        string[]? sortBy) where TEntity : class
    {
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
