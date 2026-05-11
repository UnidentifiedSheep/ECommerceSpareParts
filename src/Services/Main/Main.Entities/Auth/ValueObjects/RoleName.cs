using Domain.Extensions;

namespace Main.Entities.Auth.ValueObjects;

public record RoleName
{
    private RoleName()
    {
    }

    public RoleName(string name)
    {
        Value = name
            .Trim()
            .AgainstTooShort(3, "role.name.min.length")
            .AgainstTooLong(24, "role.name.max.length");

        Value = ToNormalized(Value);
    }

    public string Value { get; } = null!;

    public static string ToNormalized(string source)
    {
        return source.Trim().ToUpperInvariant();
    }

    public static implicit operator RoleName(string value)
    {
        return new RoleName(value);
    }

    public static implicit operator string(RoleName sku)
    {
        return sku.Value;
    }
}