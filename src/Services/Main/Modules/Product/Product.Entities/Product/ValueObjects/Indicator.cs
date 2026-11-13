using Domain.Validation;

namespace Main.Entities.Product.ValueObjects;

public record Indicator
{
	private Indicator()
	{
	}

	public Indicator(string? value)
	{
		value = value?.Trim();
		value?.EnsureMaxLength(24, ArticleIndicatorMaxLength24Message.Instance);

		Value = string.IsNullOrWhiteSpace(value) ? null : value;
	}

	public string? Value { get; }

	public static implicit operator Indicator(string? value) => new(value);

	public static implicit operator string?(Indicator? indicator) => indicator?.Value;
}
