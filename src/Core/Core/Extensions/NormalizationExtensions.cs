using System.Text.RegularExpressions;

namespace Core.Extensions;

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
}