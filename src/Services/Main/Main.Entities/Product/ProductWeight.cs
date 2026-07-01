using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Enums;

namespace Main.Entities.Product;

public class ProductWeight : Entity<ProductWeight, int>, ILinqEntity<ProductWeight, int>
{
    private ProductWeight() { }

    private ProductWeight(
        int productId,
        decimal weight,
        WeightUnit unit)
    {
        ValidateWeight(weight);

        ProductId = productId;
        Weight = weight;
        Unit = unit;
    }

    [Validate]
    public int ProductId { get; }

    public decimal Weight { get; private set; }

    public WeightUnit Unit { get; private set; }

    public static Expression<Func<ProductWeight, int>> GetKeySelector() { return x => x.ProductId; }

    public static Expression<Func<ProductWeight, bool>> GetEqualityExpression(int key)
    {
        return x => x.ProductId == key;
    }

    public static ProductWeight Create(
        int productId,
        decimal weight,
        WeightUnit unit)
    {
        return new ProductWeight(
            productId,
            weight,
            unit);
    }

    public void Update(decimal weight, WeightUnit unit)
    {
        ValidateWeight(weight);
        Weight = weight;
        Unit = unit;
    }

    public override int GetId() { return ProductId; }

    private static void ValidateWeight(decimal weight)
    {
        weight.AgainstLessOrEqual(0, "article.weight.must.be.greater.than.zero")
            .AgainstTooManyDecimalPlaces(2, "article.weight.max.two.decimals");
    }
}