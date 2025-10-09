using System.Text.RegularExpressions;

namespace Core.Extensions;

public static partial class NormalizationExtensions
{
    [GeneratedRegex("[^a-zA-Z0-9а-яА-Я]+")]
    private static partial Regex OnlyCharacter();

    [GeneratedRegex(@"\D")]
    private static partial Regex OnlyDigitsRegex();

    /// <summary>
    ///     Возвращает нормализованый артикул те состоящий только из букв и цифр.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string ToNormalizedArticleNumber(this string source)
    {
        return OnlyCharacter().Replace(source, "").ToUpperInvariant();
    }

    public static string ToNormalized(this string source)
    {
        return source.Trim().ToUpperInvariant();
    }

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