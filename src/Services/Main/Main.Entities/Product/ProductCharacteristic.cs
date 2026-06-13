using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Product;

public class ProductCharacteristic : Entity<ProductCharacteristic, (int, string)>,
    ILinqEntity<ProductCharacteristic, (int, string)>
{
    private ProductCharacteristic()
    {
    }

    private ProductCharacteristic(int productId, string name, string value)
    {
        ProductId = productId;
        SetName(name);
        SetValue(value);
    }

    public int ProductId { get; }

    public string Name { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    public static Expression<Func<ProductCharacteristic, (int, string)>> GetKeySelector()
    {
        return x => ValueTuple.Create(x.ProductId, x.Name);
    }

    public static Expression<Func<ProductCharacteristic, bool>> GetEqualityExpression((int, string) key)
    {
        return x => x.ProductId == key.Item1 && x.Name == key.Item2;
    }

    public static ProductCharacteristic Create(int productId, string name, string value)
    {
        return new ProductCharacteristic(productId, name, value);
    }

    public void SetValue(string value)
    {
        Value = value.Trim()
            .AgainstTooShort(3, "article.characteristic.value.min.length")
            .AgainstTooLong(128, "article.characteristic.value.max.length");
    }

    public void SetName(string name)
    {
        Name = name.Trim()
            .AgainstNullOrWhiteSpace(() =>
                throw new InvalidOperationException("Product characteristic name cannot be null or empty."))
            .AgainstTooLong(128, "article.characteristic.name.max.length");
    }

    public override (int, string) GetId()
    {
        return (ProductId, Name);
    }
}
