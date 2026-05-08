using Domain.Extensions;

namespace Main.Entities.Product.ValueObjects;

public record Name
{
    private Name()
    {
    }

    public Name(string value)
    {
        value = value.Trim();

        value.AgainstNullOrWhiteSpace("producer.name.not.empty")
            .AgainstTooShort(2, "producer.name.min.length")
            .AgainstTooLong(64, "producer.name.max.length");

        Value = char.ToUpperInvariant(value[0]) + value[1..];
    }

    public string Value { get; } = null!;

    public static implicit operator Name(string value)
    {
        return new Name(value);
    }

    public static implicit operator string(Name name)
    {
        return name.Value;
    }
}