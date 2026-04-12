using System.Text.RegularExpressions;

namespace Extensions;

public static partial class NormalizationExtensions
{
    [GeneratedRegex("[^a-zA-Z0-9а-яА-Я]+")]
    public static partial Regex OnlyCharacter();

    [GeneratedRegex(@"\D")]
    public static partial Regex OnlyDigitsRegex();

    public static string ToNormalizedEmail(this string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    public static string ToNormalizedPhoneNumber(this string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return source;
        return OnlyDigitsRegex().Replace(source.Trim(), "");
    }

    public static string ToNormalizedPermission(this string permission)
    {
        return permission.ToUpperInvariant().Replace('_', '.');
    }

    public static string ToNormalizedPermission(this Enum permission)
    {
        return permission.ToString().ToUpperInvariant().Replace('_', '.');
    }

    public static string ToNormalized(this string source)
    {
        return source.Trim().ToUpperInvariant();
    }
}