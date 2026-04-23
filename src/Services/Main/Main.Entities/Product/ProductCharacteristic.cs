using Domain;
using Domain.Extensions;

namespace Main.Entities.Product;

public class ProductCharacteristic : Entity<ProductCharacteristic, (int, string)>
{
    public int ProductId { get; private set; }

    public string Name { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    
    private ProductCharacteristic() {}

    private ProductCharacteristic(int productId, string name, string value)
    {
        ProductId = productId;
        SetName(name);
        SetValue(value);
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
            .AgainstNullOrWhiteSpace(() => throw new InvalidOperationException("Product characteristic name cannot be null or empty."))
            .AgainstTooLong(128, "article.characteristic.name.max.length");
    }

    public override (int, string) GetId() => (ProductId, Name);
}