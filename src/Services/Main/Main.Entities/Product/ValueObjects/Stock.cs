namespace Main.Entities.Product.ValueObjects;

public record Stock
{
    public int Value { get; }
    
    private Stock() {}

    public Stock(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }
    
    public static implicit operator Stock(int value) => new(value);

    public static implicit operator int(Stock stock) => stock.Value;
}