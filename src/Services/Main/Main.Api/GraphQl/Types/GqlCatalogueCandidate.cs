using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Application.Dtos.Product.Enrichment;

namespace Main.Api.GraphQl.Types;

[GraphQLName("CatalogueCandidate")]
public record GqlCatalogueCandidate(
    [property: GraphQLIgnore]
    CatalogueCandidateReviewDto CatalogueCandidateDto)
{
    [GraphQLName("id")]
    [Shareable]
    public Guid Id => CatalogueCandidateDto.Id;

    [GraphQLName("producer")]
    public GqlProducer Producer => new(CatalogueCandidateDto.Producer);

    [GraphQLName("product")]
    public GqlProduct? Product => CatalogueCandidateDto.Product == null 
        ? null
        : new GqlProduct(CatalogueCandidateDto.Product);

    [GraphQLName("sku")]
    public string Sku => CatalogueCandidateDto.Sku;
    
    [GraphQLName("supplierProducts")]
    public IReadOnlyList<GqlSupplierProduct> SupplierProducts
        => CatalogueCandidateDto
            .SupplierProducts
            .Select(z => new GqlSupplierProduct(z))
            .ToList();

}