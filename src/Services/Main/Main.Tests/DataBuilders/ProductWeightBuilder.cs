using Bogus;
using Enums;
using Main.Entities.Product;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class ProductWeightBuilder(Faker faker) : BuilderBase<ProductWeight>(faker)
{
    public int? ProductId { get; private set; }
    public decimal? Weight { get; private set; }
    public WeightUnit? Unit { get; private set; }

    public ProductWeightBuilder WithProductId(int productId)
    {
        ProductId = productId;
        return this;
    }

    public ProductWeightBuilder WithWeight(decimal weight)
    {
        Weight = weight;
        return this;
    }

    public ProductWeightBuilder WithUnit(WeightUnit unit)
    {
        Unit = unit;
        return this;
    }

    public override ProductWeight Build()
    {
        return ProductWeight.Create(
            ProductId ?? Faker.Random.Int(1),
            Weight ?? 2m,
            Unit ?? WeightUnit.Kilogram);
    }
}
