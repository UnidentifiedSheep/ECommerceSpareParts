using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Product.Reservation;
using Main.Entities.Product;
using Main.Entities.Storage;

namespace Main.Application.Projections;

public static class ProductProjections
{
    public static readonly Expression<Func<Product, ProductDto>> ToDto =
        x => new ProductDto
        {
            Id = x.Id,
            Name = x.Name,
            Sku = x.Sku,
            Description = x.Description,
            Stock = x.Stock,
            ProducerId = x.ProducerId,
            ProducerName = x.Producer.Name,
            Indicator = x.Indicator,
            Images = x.Images.Select(z => z.Path).ToList()
        };

    public static readonly Expression<Func<Product, FullProductDto>> ToFullDto =
        x => new FullProductDto
        {
            Id = x.Id,
            Name = x.Name,
            Sku = x.Sku,
            Description = x.Description,
            Stock = x.Stock,
            ProducerId = x.ProducerId,
            ProducerName = x.Producer.Name,
            Indicator = x.Indicator,
            Images = x.Images.Select(z => z.Path).ToList(),
            ProductWeight = ToProductWeightDto.Invoke(x.ProductWeight),
            ProductSize = ToProductSizeDto.Invoke(x.ProductSize)
        };

    public static readonly Expression<Func<ProductWeight?, ProductWeightDto?>>
        ToProductWeightDto =
            x => x == null
                ? null
                : new ProductWeightDto
                {
                    ProductId = x.ProductId,
                    Weight = x.Weight,
                    Unit = x.Unit
                };

    public static readonly Expression<Func<ProductSize?, ProductSizeDto?>>
        ToProductSizeDto =
            x => x == null
                ? null
                : new ProductSizeDto
                {
                    ProductId = x.ProductId,
                    Unit = x.Unit,
                    Length = x.Length,
                    Height = x.Height,
                    Width = x.Width,
                    VolumeM3 = x.VolumeM3
                };

    public static readonly Expression<Func<ProductReservation, ProductReservationDto>>
        ToReservationDto =
            x => new ProductReservationDto
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
                Organization = OrganizationProjections.ToDto.Invoke(x.Organization)
            };
}
