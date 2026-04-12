using Domain.Extensions;

namespace Main.Entities.Product.ValueObjects;

public record Name
{
    public string Value { get; } = null!;
    
    private Name() {}

    public Name(string value)
    {
        value = value.Trim();

        value.AgainstNullOrWhiteSpace("article.name.must.not.be.empty")
            .AgainstTooShort(3, "article.name.min.length.3")
            .AgainstTooLong(255, "article.name.max.length.255");

        Value = char.ToUpperInvariant(value[0]) + value[1..];
    }
    
    public static implicit operator Name(string value) => new(value);

    public static implicit operator string(Name name) => name.Value;
}