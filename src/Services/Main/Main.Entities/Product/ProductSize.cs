using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Enums;
using Extensions;

namespace Main.Entities.Product;

public class ProductSize : Entity<ProductSize, int>, ILinqEntity<ProductSize, int>
{
    private ProductSize()
    {
    }

    private ProductSize(int productId, decimal length, decimal width, decimal height, DimensionUnit unit)
    {
        ProductId = productId;
        SetLength(length);
        SetWidth(width);
        SetHeight(height);
        SetUnit(unit);
    }

    [Validate]
    public int ProductId { get; }

    public decimal Length { get; private set; }

    public decimal Width { get; private set; }

    public decimal Height { get; private set; }

    public DimensionUnit Unit { get; private set; }

    public decimal VolumeM3 { get; private set; }

    public static Expression<Func<ProductSize, int>> GetKeySelector()
    {
        return x => x.ProductId;
    }

    public static Expression<Func<ProductSize, bool>> GetEqualityExpression(int key)
    {
        return x => x.ProductId == key;
    }

    public static ProductSize Create(int productId, decimal length, decimal width, decimal height, DimensionUnit unit)
    {
        var size = new ProductSize(productId, length, width, height, unit);
        return size;
    }

    public void SetLength(decimal length)
    {
        length.AgainstLessOrEqual(0, "article.size.length.must.be.greater.than.zero")
            .AgainstTooManyDecimalPlaces(2, "article.size.length.max.two.decimals");

        Length = length;
        RecalculateVolume();
    }

    public void SetWidth(decimal width)
    {
        width.AgainstLessOrEqual(0, "article.size.width.must.be.greater.than.zero")
            .AgainstTooManyDecimalPlaces(2, "article.size.width.max.two.decimals");
        Width = width;
        RecalculateVolume();
    }

    public void SetHeight(decimal height)
    {
        height.AgainstLessOrEqual(0, "article.size.height.must.be.greater.than.zero")
            .AgainstTooManyDecimalPlaces(2, "article.size.height.max.two.decimals");

        Height = height;
        RecalculateVolume();
    }

    public void SetUnit(DimensionUnit unit)
    {
        Unit = unit;
        RecalculateVolume();
    }

    private void RecalculateVolume()
    {
        VolumeM3 = DimensionExtensions.ToCubicMeters(Length, Width, Height, Unit);
    }

    public override int GetId()
    {
        return ProductId;
    }
}
