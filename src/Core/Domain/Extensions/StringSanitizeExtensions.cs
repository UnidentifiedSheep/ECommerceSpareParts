namespace Domain.Extensions;

public static class StringSanitizeExtensions
{
    public static string? NullIfWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    
    public static string TrimSafe(this string? value)
        => value?.Trim() ?? string.Empty;

    public static string? TrimOrNull(this string? value)
        => value?.Trim();
    
    public static string? IfNotNull(this string? value, Func<string, string> transform)
        => value is null ? null : transform(value);

    public static string? IfNotNullOrWhiteSpace(this string? value, Func<string, string> transform)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return transform(value.Trim());
    }
}