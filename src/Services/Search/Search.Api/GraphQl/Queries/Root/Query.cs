using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;

namespace Search.Api.GraphQl.Queries.Root;

public sealed class Query
{
    [GraphQLName("catalogue")]
    public CatalogueQueries Catalogue => new();
    
    [GraphQLName("producers")]
    public ProducerQueries Producer => new();
}