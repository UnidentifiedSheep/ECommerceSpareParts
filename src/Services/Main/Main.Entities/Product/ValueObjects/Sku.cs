using System.Text.RegularExpressions;
using Domain.Extensions;

namespace Main.Entities.Product.ValueObjects;

public partial record Sku
{
    private Sku() { }

    public Sku(string value)
    {
        value = value.Trim();

        value.EnsureNotNullOrEmpty("article.articleNumber.must.not.be.empty")
            .EnsureMinLength(3, "article.articleNumber.min.length.3")
            .EnsureMaxLength(128, "article.articleNumber.max.length.128");

        Value = value;
        NormalizedValue = ToNormalized(Value);
    }

    public string Value { get; } = null!;
    public string NormalizedValue { get; } = null!;

    [GeneratedRegex("[^a-zA-Z0-9а-яА-Я]+")]
    private static partial Regex OnlyCharacter();

    public static string ToNormalized(string source)
    {
        return OnlyCharacter().Replace(source, "").ToUpperInvariant();
    }

    public static bool IsValid(string? sku, out Exception? exception)
    {
        exception = null;
        var value = sku?.Trim();

        return true;
    }

    public static implicit operator Sku(string value) { return new Sku(value); }

    public static implicit operator string(Sku sku) { return sku.Value; }
}
