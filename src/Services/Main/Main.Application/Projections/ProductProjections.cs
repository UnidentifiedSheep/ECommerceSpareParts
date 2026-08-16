using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Application.Common.Models.Options.S3;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Product.Reservation;
using Main.Application.Dtos.Organizations;
using Main.Entities.Organization;
using Main.Entities.Product;
using Main.Entities.Storage;
using Microsoft.Extensions.Options;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class ProductDtoProjectionProvider
    : ProjectionProviderBase<Product, ProductDto>
{
    public ProductDtoProjectionProvider(IOptions<S3BucketsOptions> bucketsOptions)
    {
        var imagesBaseUrl = bucketsOptions.Value.Images.PublicBaseUrl.TrimEnd('/') + "/";

        Projection = x => new ProductDto
        {
            Id = x.Id,
            Name = x.Name,
            Sku = x.Sku,
            Description = x.Description,
            Stock = x.Stock,
            ProducerId = x.ProducerId,
            ProducerName = x.Producer.Name,
            Indicator = x.Indicator,
            Images = x.Images.Select(z => imagesBaseUrl + z.StorageKey).ToList()
        };
    }

    public override Expression<Func<Product, ProductDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class FullProductDtoProjectionProvider
    : ProjectionProviderBase<Product, FullProductDto>
{
    public FullProductDtoProjectionProvider(
        IOptions<S3BucketsOptions> bucketsOptions,
        IProjectionProvider<ProductWeight, ProductWeightDto> productWeightProjection,
        IProjectionProvider<ProductSize, ProductSizeDto> productSizeProjection)
    {
        var imagesBaseUrl = bucketsOptions.Value.Images.PublicBaseUrl.TrimEnd('/') + "/";
        var weightToDto = productWeightProjection.Projection;
        var sizeToDto = productSizeProjection.Projection;

        Projection = x => new FullProductDto
        {
            Id = x.Id,
            Name = x.Name,
            Sku = x.Sku,
            Description = x.Description,
            Stock = x.Stock,
            ProducerId = x.ProducerId,
            ProducerName = x.Producer.Name,
            Indicator = x.Indicator,
            Images = x.Images.Select(z => imagesBaseUrl + z.StorageKey).ToList(),
            ProductWeight = x.ProductWeight == null
                ? null
                : weightToDto.Invoke(x.ProductWeight),
            ProductSize = x.ProductSize == null
                ? null
                : sizeToDto.Invoke(x.ProductSize)
        };
    }

    public override Expression<Func<Product, FullProductDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProductWeightDtoProjectionProvider
    : ProjectionProviderBase<ProductWeight, ProductWeightDto>
{
    public override Expression<Func<ProductWeight, ProductWeightDto>> Projection { get; } =
        x => new ProductWeightDto
        {
            ProductId = x.ProductId,
            Weight = x.Weight,
            Unit = x.Unit
        };
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProductSizeDtoProjectionProvider
    : ProjectionProviderBase<ProductSize, ProductSizeDto>
{
    public override Expression<Func<ProductSize, ProductSizeDto>> Projection { get; } =
        x => new ProductSizeDto
        {
            ProductId = x.ProductId,
            Unit = x.Unit,
            Length = x.Length,
            Height = x.Height,
            Width = x.Width,
            VolumeM3 = x.VolumeM3
        };
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProductReservationDtoProjectionProvider
    : ProjectionProviderBase<ProductReservation, ProductReservationDto>
{
    public ProductReservationDtoProjectionProvider(
        IProjectionProvider<Organization, OrganizationDto> organizationProjection)
    {
        var organizationToDto = organizationProjection.Projection;

        Projection = x => new ProductReservationDto
        {
            Id = x.Id,
            WhoUpdated = x.WhoUpdated,
            Comment = x.Comment,
            CurrentCount = x.CurrentCount,
            ProposedCurrencyId = x.ProposedCurrencyId,
            ProposedPrice = x.ProposedPrice,
            ReservedCount = x.ReservedCount,
            Status = x.Status,
            UpdatedAt = x.UpdatedAt,
            Organization = organizationToDto.Invoke(x.Organization)
        };
    }

    public override Expression<Func<ProductReservation, ProductReservationDto>> Projection { get; }
}
