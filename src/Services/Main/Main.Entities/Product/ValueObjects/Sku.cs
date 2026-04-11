namespace Main.Entities.Product.ValueObjects;

public record Sku
{
    public string Value { get; } = null!;
    public string NormalizedValue { get; } = null!;

    private Sku() {}

    public Sku(string value)
    {
        value = value.Trim();
        
    }
}