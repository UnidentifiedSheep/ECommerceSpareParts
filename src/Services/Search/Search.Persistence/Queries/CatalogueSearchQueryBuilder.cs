using Extensions;
using OpenSearch.Client;
using Search.Application.Models.CatalogueSearch;
using Search.Enums;

namespace Search.Persistence.Queries;

internal static class CatalogueSearchQueryBuilder
{
    internal static QueryContainer Build<TDocument>(
        QueryContainerDescriptor<TDocument> query,
        CatalogueSearchCriteria criteria,
        Field normalizedSkuField,
        Field nameField,
        Field producerIdField)
        where TDocument : class
    {
        var filters = BuildFilters<TDocument>(criteria, producerIdField);
        if (string.IsNullOrWhiteSpace(criteria.Query))
            return filters.Count == 0
                ? query.MatchAll()
                : query.Bool(boolean => boolean.Filter(filters));

        var normalizedSku = criteria.Query.OnlyCharacterToLower();
        var textQuery = criteria.Query.Trim();
        var should = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
        var skuQueries = BuildSkuQueries<TDocument>(
            normalizedSku,
            criteria.SkuModes,
            normalizedSkuField);
        var nameQueries = BuildNameQueries<TDocument>(
            textQuery,
            criteria.NameModes,
            nameField);

        if (skuQueries.Count > 0)
            should.Add(descriptor => descriptor.DisMax(disMax => disMax
                .Queries(skuQueries.ToArray())
                .TieBreaker(0.1)));

        if (nameQueries.Count > 0)
            should.Add(descriptor => descriptor.DisMax(disMax => disMax
                .Queries(nameQueries.ToArray())
                .TieBreaker(0.1)));

        if (should.Count == 0)
            return query.MatchNone();

        return query.Bool(boolean =>
        {
            boolean = boolean
                .Should(should.ToArray())
                .MinimumShouldMatch(1);

            return filters.Count == 0
                ? boolean
                : boolean.Filter(filters);
        });
    }

    private static List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>> BuildSkuQueries<TDocument>(
        string query,
        IReadOnlySet<SearchMatchType> modes,
        Field field)
        where TDocument : class
    {
        var queries = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
        if (string.IsNullOrEmpty(query)) return queries;

        if (modes.Contains(SearchMatchType.Exact))
            queries.Add(descriptor => descriptor.Term(term => term
                .Field(field)
                .Value(query)
                .Boost(100)));

        if (query.Length >= 2 && modes.Contains(SearchMatchType.StartsWith))
            queries.Add(descriptor => descriptor.Match(match => match
                .Field(new Field($"{field.Name}.prefix"))
                .Query(query)
                .Boost(50)));

        if (query.Length >= 2 && modes.Contains(SearchMatchType.Contains))
            queries.Add(descriptor => descriptor.Match(match => match
                .Field(new Field($"{field.Name}.contains"))
                .Query(query)
                .Boost(20)));

        if (query.Length >= 4 && modes.Contains(SearchMatchType.Fuzzy))
            queries.Add(descriptor => descriptor.Fuzzy(fuzzy => fuzzy
                .Field(field)
                .Value(query)
                .Fuzziness(Fuzziness.Auto)
                .PrefixLength(1)
                .Boost(5)));

        return queries;
    }

    private static List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>> BuildNameQueries<TDocument>(
        string query,
        IReadOnlySet<SearchMatchType> modes,
        Field field)
        where TDocument : class
    {
        var queries = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
        if (string.IsNullOrWhiteSpace(query)) return queries;

        if (modes.Contains(SearchMatchType.Exact))
            queries.Add(descriptor => descriptor.Term(term => term
                .Field(new Field($"{field.Name}.keyword"))
                .Value(query.ToLowerInvariant())
                .Boost(30)));

        if (query.Length >= 2 && modes.Contains(SearchMatchType.StartsWith))
            queries.Add(descriptor => descriptor.Prefix(prefix => prefix
                .Field(new Field($"{field.Name}.keyword"))
                .Value(query.ToLowerInvariant())
                .Boost(15)));

        if (query.Length >= 2 && modes.Contains(SearchMatchType.Contains))
            queries.Add(descriptor => descriptor.Wildcard(wildcard => wildcard
                .Field(new Field($"{field.Name}.keyword"))
                .Value($"*{EscapeWildcard(query.ToLowerInvariant())}*")
                .Boost(8)));

        if (query.Length >= 4 && modes.Contains(SearchMatchType.Fuzzy))
            queries.Add(descriptor => descriptor.Match(match => match
                .Field(field)
                .Query(query)
                .Operator(Operator.And)
                .Fuzziness(Fuzziness.Auto)
                .PrefixLength(1)
                .Boost(2)));

        return queries;
    }

    private static string EscapeWildcard(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("*", "\\*", StringComparison.Ordinal)
            .Replace("?", "\\?", StringComparison.Ordinal);
    }

    private static List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>> BuildFilters<TDocument>(
        CatalogueSearchCriteria criteria,
        Field producerIdField)
        where TDocument : class
    {
        var filters = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();
        if (criteria.ProducerIds.Count == 0) return filters;

        filters.Add(descriptor => descriptor.Terms(terms => terms
            .Field(producerIdField)
            .Terms(criteria.ProducerIds.Select(id => (object)id))));
        return filters;
    }
}
