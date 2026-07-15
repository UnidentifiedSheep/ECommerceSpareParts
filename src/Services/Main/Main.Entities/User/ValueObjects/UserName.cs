using Domain.Extensions;

namespace Main.Entities.User.ValueObjects;

public record UserName
{
    private UserName() { }

    public UserName(string value)
    {
        value = value.Trim();

        value.EnsureNotNullOrEmpty("login.must.not.be.empty")
            .EnsureMinLength(5, "login.min.length.5")
            .EnsureMaxLength(36, "login.max.length.36")
            .EnsureNoSpaces("login.cannot.contain.spaces");

        Value = value;
        NormalizedValue = ToNormalized(Value);
    }

    public string Value { get; } = null!;
    public string NormalizedValue { get; } = null!;

    public static string ToNormalized(string source) { return source.Trim().ToUpperInvariant(); }

    public static implicit operator UserName(string value) { return new UserName(value); }

    public static implicit operator string(UserName sku) { return sku.Value; }
}