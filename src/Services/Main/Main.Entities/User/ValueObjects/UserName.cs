using Domain.Extensions;

namespace Main.Entities.User.ValueObjects;

public record UserName
{
    private UserName() { }

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

    public string Value { get; } = null!;
    public string NormalizedValue { get; } = null!;

    public static string ToNormalized(string source) { return source.Trim().ToUpperInvariant(); }

    public static implicit operator UserName(string value) { return new UserName(value); }

    public static implicit operator string(UserName sku) { return sku.Value; }
}