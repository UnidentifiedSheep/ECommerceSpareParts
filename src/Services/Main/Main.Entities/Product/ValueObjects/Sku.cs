using System.Text.RegularExpressions;
using Domain.Extensions;

namespace Main.Entities.Product.ValueObjects;

public partial record Sku
{
    public string Value { get; } = null!;
    public string NormalizedValue { get; } = null!;

    private Sku() {}

    public Sku(string value)
    {
        value = value.Trim();

        value.AgainstNullOrEmpty("article.articleNumber.must.not.be.empty")
            .AgainstTooShort(3, "article.articleNumber.min.length.3")
            .AgainstTooLong(128, "article.articleNumber.max.length.128");

        Value = value;
        NormalizedValue = ToNormalized(Value);
    }
    
    [GeneratedRegex("[^a-zA-Z0-9а-яА-Я]+")]
    private static partial Regex OnlyCharacter();

    public string ToNormalized(string source)
    {
        return OnlyCharacter().Replace(source, "").ToUpperInvariant();
    }
    
    public static implicit operator Sku(string value) => new(value);

    public static implicit operator string(Sku sku) => sku.Value;
}