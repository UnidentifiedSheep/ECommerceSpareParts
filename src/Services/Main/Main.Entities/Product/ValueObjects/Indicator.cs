using Domain.Extensions;

namespace Main.Entities.Product.ValueObjects;

public record Indicator
{
    public string? Value { get; }
    
    private Indicator() {}

    public Indicator(string? value)
    {
        value = value?.Trim();
        value?.AgainstTooLong(24, "article.indicator.max.length.24");
        
        Value = string.IsNullOrWhiteSpace(value) ? null : value;
    }
    
    public static implicit operator Indicator(string? value) => new(value);

    public static implicit operator string?(Indicator? indicator) => indicator?.Value;
}