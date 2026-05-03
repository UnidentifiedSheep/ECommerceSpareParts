using Domain.Extensions;

namespace Main.Entities.Product.ValueObjects;

public record Stock
{
    public int Value { get; }
    
    private Stock() {}

    public Stock(int value)
    {
        value.AgainstNegative(() => new InvalidOperationException("Stock can not be negative"));
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }
    
    public static implicit operator Stock(int value) => new(value);

    public static implicit operator int(Stock stock) => stock.Value;
}