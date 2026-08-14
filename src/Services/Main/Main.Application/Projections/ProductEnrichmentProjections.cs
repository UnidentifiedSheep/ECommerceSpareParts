using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Contracts.Models.CatalogueCandidate;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Product.Enrichment;
using Main.Entities.Product;
using Main.Entities.Product.Enrichment;
using ProducerEntity = Main.Entities.Producer.Producer;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class SupplierProductNameDtoProjectionProvider
    : ProjectionProviderBase<SupplierProductName, SupplierProductNameDto>
{
    public override Expression<Func<SupplierProductName, SupplierProductNameDto>> Projection { get; } =
        x => new SupplierProductNameDto
        {
            Id = x.Id,
            SupplierProductId = x.SupplierProductId,
            Name = x.Name
        };
}

[Lifetime(Lifetime.Singleton)]
public sealed class SupplierProductDtoProjectionProvider
    : ProjectionProviderBase<SupplierProduct, SupplierProductDto>
{
    public SupplierProductDtoProjectionProvider(
        IProjectionProvider<SupplierProductName, SupplierProductNameDto> nameProjection)
    {
        var nameToDto = nameProjection.Projection;

        Projection = x => new SupplierProductDto
        {
            Id = x.Id,
            Sku = x.Sku.Value,
            Producer = x.Producer,
            Supplier = x.Supplier,
            Names = x.Names
                .OrderBy(z => z.Id)
                .Select(z => nameToDto.Invoke(z))
                .ToList()
        };
    }

    public override Expression<Func<SupplierProduct, SupplierProductDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class CatalogueCandidateReviewDtoProjectionProvider
    : ProjectionProviderBase<CatalogueCandidate, CatalogueCandidateReviewDto>
{
    public CatalogueCandidateReviewDtoProjectionProvider(
        IProjectionProvider<ProducerEntity, ProducerDto> producerProjection,
        IProjectionProvider<Product, ProductDto> productProjection,
        IProjectionProvider<SupplierProduct, SupplierProductDto> supplierProductProjection)
    {
        var producerToDto = producerProjection.Projection;
        var productToDto = productProjection.Projection;
        var supplierProductToDto = supplierProductProjection.Projection;

        Projection = x => new CatalogueCandidateReviewDto
        {
            Id = x.Id,
            Producer = producerToDto.Invoke(x.Producer),
            Product = x.Product == null
                ? null
                : productToDto.Invoke(x.Product),
            Sku = x.Sku.Value,
            SupplierProducts = x.SupplierProducts
                .OrderBy(z => z.Id)
                .Select(z => supplierProductToDto.Invoke(z))
                .ToList()
        };
    }

    public override Expression<Func<CatalogueCandidate, CatalogueCandidateReviewDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class CatalogueCandidateContractDtoProjectionProvider
    : ProjectionProviderBase<CatalogueCandidate, CatalogueCandidateContractDto>
{
    public CatalogueCandidateContractDtoProjectionProvider()
    {
        Projection = x => new CatalogueCandidateContractDto
        {
            Id = x.Id,
            Sku = x.Sku.Value,
            MappedProductId = x.ProductId,
            ProducerId = x.ProducerId,
            Names = x.SupplierProducts
                .SelectMany(z => z.Names
                    .Select(c => c.Name.Trim()))
                .Distinct()
                .ToList()
        };
    }
    
    public override Expression<Func<CatalogueCandidate, CatalogueCandidateContractDto>> Projection { get; }
}