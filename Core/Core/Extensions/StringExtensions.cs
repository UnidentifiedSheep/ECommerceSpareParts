using System.Text.RegularExpressions;

namespace Core.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex("[^a-zA-Z0-9а-яА-Я]+")]
    private static partial Regex OnlyCharacter();

    [GeneratedRegex(@"^\+?[1-9]\d{9,14}$")]
    private static partial Regex PhoneNumberRegex();

    [GeneratedRegex(@"\D")]
    private static partial Regex OnlyDigitsRegex();

    /// <summary>
    ///     Возвращает нормализованый артикул те состоящий только из букв и цифр.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string ToNormalizedArticleNumber(this string source)
    {
        return OnlyCharacter().Replace(source, "").ToUpper();
    }

    public static string OnlyDigitsCharsWithSpaces(this string source)
    {
        return OnlyCharacter().Replace(source, " ");
    }

    public static string? ToNormalized(this string? source)
    {
        return source?.Trim().ToUpperInvariant();
    }

    public static bool IsValidPhoneNumber(this string phoneNumber)
    {
        var onlyDigits = OnlyDigitsRegex().Replace(phoneNumber, "");
        return PhoneNumberRegex().IsMatch(onlyDigits);
    }

    public static string ToNormalizedPhoneNumber(this string source)
    {
        return OnlyDigitsRegex().Replace(source, "");
    }

    //Must contain 11 digits.
    public static string ToBeautifulNumber(this string source)
    {
        var normalizedNumber = source.ToNormalizedPhoneNumber();
        string countryCode;
        switch (normalizedNumber.Length)
        {
            case 11:
            {
                countryCode = normalizedNumber[..1];
                normalizedNumber = normalizedNumber.Remove(0, 1);
                break;
            }
            case 12:
            {
                countryCode = normalizedNumber[..2];
                normalizedNumber = normalizedNumber.Remove(0, 2);
                break;
            }
            case 13:
            {
                countryCode = normalizedNumber[..3];
                normalizedNumber = normalizedNumber.Remove(0, 3);
                break;
            }
            default:
                return normalizedNumber;
        }

        var n = normalizedNumber;
        var beautifulNumber = $"+{countryCode} ({n[..3]}) {n[3..6]}-{n[6..8]}-{n[8..10]}";
        return beautifulNumber;
    }
}