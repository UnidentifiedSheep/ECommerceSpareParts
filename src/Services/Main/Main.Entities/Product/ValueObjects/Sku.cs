using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Domain.Extensions;
using Exceptions;

namespace Main.Entities.Product.ValueObjects;

public partial record Sku
{
    private Sku() { }

    public Sku(string value)
    {
        value = value.Trim();

        if (!IsValid(value, out var exception))
            throw exception;

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

    public static bool IsValid(string? sku, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (string.IsNullOrWhiteSpace(sku))
        {
            exception = new InvalidInputException("article.articleNumber.must.not.be.empty");
            return false;
        }

        if (!sku.HasMinLength(3))
        {
            exception = new InvalidInputException("article.articleNumber.min.length.3");
            return false;
        }

        if (sku.HasMaxLength(128))
        {
            exception = new InvalidInputException("article.articleNumber.max.length.128");
            return false;
        }
        return true;
    }

    public static implicit operator Sku(string value) { return new Sku(value); }

    public static implicit operator string(Sku sku) { return sku.Value; }
}
