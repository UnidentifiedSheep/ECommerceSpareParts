using HotChocolate;

namespace Main.Api.GraphQl.Queries.Root;

public sealed class Query
{
    [GraphQLName("products")]
    public ProductQueries ProductQueries => new();
    
    [GraphQLName("catalogueCandidates")]
    public CatalogueCandidateQueries CatalogueCandidateQueries => new();
}