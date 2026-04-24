using Domain.Extensions;

namespace Main.Entities.User.ValueObjects;

public record UserName
{
    public string Value { get; } = null!;
    public string NormalizedValue { get; } = null!;

    private UserName() {}

    public UserName(string value)
    {
        value = value.Trim();

        value.AgainstNullOrEmpty("login.must.not.be.empty")
            .AgainstTooShort(5, "login.min.length.5")
            .AgainstTooLong(36, "login.max.length.36")
            .AgainstSpaces("login.cannot.contain.spaces");

        Value = value;
        NormalizedValue = ToNormalized(Value);
    }

    public static string ToNormalized(string source)
    {
        return source.Trim().ToUpperInvariant();
    }
    
    public static implicit operator UserName(string value) => new(value);

    public static implicit operator string(UserName sku) => sku.Value;
}