using Domain.Extensions;

namespace Main.Entities.Auth.ValueObjects;

public record RoleName
{
    public string Value { get; } = null!;
    public string NormalizedValue { get; } = null!;
    
    private RoleName() {}

    private RoleName(string name)
    {
        Value = name
            .Trim()
            .AgainstTooShort(3, "role.name.min.length")
            .AgainstTooLong(24, "role.name.max.length");
        
        NormalizedValue = ToNormalized(Value);
    }
        
    public static string ToNormalized(string source)
    {
        return source.Trim().ToUpperInvariant();
    }
    
    public static implicit operator RoleName(string value) => new(value);

    public static implicit operator string(RoleName sku) => sku.Value;
}