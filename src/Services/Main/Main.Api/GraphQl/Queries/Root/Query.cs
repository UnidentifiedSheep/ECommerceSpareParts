using HotChocolate;

namespace Main.Api.GraphQl.Queries.Root;

public sealed class Query
{
    [GraphQLName("products")]
    public ProductQueries Product => new();
    
    [GraphQLName("catalogueCandidates")]
    public CatalogueCandidateQueries CatalogueCandidate => new();
    
    [GraphQLName("producers")]
    public ProducerQueries Producer => new();
}