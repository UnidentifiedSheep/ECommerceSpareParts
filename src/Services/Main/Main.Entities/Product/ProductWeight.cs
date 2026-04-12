using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Enums;

namespace Main.Entities.Product;

public class ProductWeight : Entity<ProductWeight, int>
{
    [Validate]
    public int ProductId { get; private set; }

    public decimal Weight { get; private set; }

    public WeightUnit Unit { get; private set; }

    private ProductWeight() {}

    private ProductWeight(int productId, decimal weight, WeightUnit unit)
    {
        ValidateWeight(weight);
        
        ProductId = productId;
        Weight = weight;
        Unit = unit;
    }

    public static ProductWeight Create(int productId, decimal weight, WeightUnit unit)
    {
        return new ProductWeight(productId, weight, unit);
    }

    public void Update(decimal weight, WeightUnit unit)
    {
        ValidateWeight(weight);
        Weight = weight;
        Unit = unit;
    }

    public override int GetId() => ProductId;

    private static void ValidateWeight(decimal weight)
    {
        weight.AgainstTooSmall(0, "article.weight.must.be.greater.than.zero")
            .AgainstTooManyDecimalPlaces(2, "article.weight.max.two.decimals");
    }
}