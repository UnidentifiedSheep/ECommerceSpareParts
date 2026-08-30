using Domain.Validation;

namespace Main.Entities.Product.ValueObjects;

public record Stock
{
	private Stock()
	{
	}

	public Stock(int value)
	{
		value.EnsureNonNegative(() => new InvalidOperationException("Stock can not be negative"));
		ArgumentOutOfRangeException.ThrowIfNegative(value);
		Value = value;
	}

	public int Value { get; }

	public static implicit operator Stock(int value) => new(value);

	public static implicit operator int(Stock stock) => stock.Value;
}
