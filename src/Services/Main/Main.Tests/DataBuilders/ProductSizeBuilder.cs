using Bogus;
using Enums;
using Main.Entities.Product;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class ProductSizeBuilder(Faker faker) : BuilderBase<ProductSize>(faker)
{
    public int? ProductId { get; private set; }
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }
    public DimensionUnit? Unit { get; private set; }

    public ProductSizeBuilder WithProductId(int productId)
    {
        ProductId = productId;
        return this;
    }

    public ProductSizeBuilder WithDimensions(decimal length, decimal width, decimal height)
    {
        Length = length;
        Width = width;
        Height = height;
        return this;
    }

    public ProductSizeBuilder WithUnit(DimensionUnit unit)
    {
        Unit = unit;
        return this;
    }

    public override ProductSize Build()
    {
        return ProductSize.Create(
            ProductId ?? Faker.Random.Int(1),
            Length ?? 1m,
            Width ?? 1m,
            Height ?? 1m,
            Unit ?? DimensionUnit.Meter);
    }
}