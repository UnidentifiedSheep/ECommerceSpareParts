using Domain.Extensions;

namespace Main.Entities.Product.ValueObjects;

public record Name
{
    private Name() { }

    public Name(string value)
    {
        value = value.Trim();

        value.EnsureNotNullOrWhiteSpace("article.name.must.not.be.empty")
            .EnsureMinLength(3, "article.name.min.length.3")
            .EnsureMaxLength(255, "article.name.max.length.255");

        Value = char.ToUpperInvariant(value[0]) + value[1..];
    }

    public string Value { get; } = null!;

    public static implicit operator Name(string value) { return new Name(value); }

    public static implicit operator string(Name name) { return name.Value; }
}