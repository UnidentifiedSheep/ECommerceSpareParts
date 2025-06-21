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
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
    /// <summary>
    /// Возвращает нормализованый артикул те состоящий только из букв и цифр.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string ToNormalizedArticleNumber(this string source) => OnlyCharacter().Replace(source, "").ToUpper();
    public static string OnlyDigitsCharsWithSpaces(this string source) => OnlyCharacter().Replace(source, " ");
    public static bool IsValidMail(this string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return false;
        if (source.Length > 254) return false;
        var parts = source.Split('@');
        if (parts.Length != 2) return false;
        if (parts[0].Length is 0 or > 64) return false;
        if (parts[1].Length == 0) return false;
        return EmailRegex().IsMatch(source);
    }
    public static bool IsValidPhoneNumber(this string phoneNumber) => PhoneNumberRegex().IsMatch(phoneNumber);
    public static string ToNormalizedPhoneNumber(this string source) => OnlyDigitsRegex().Replace(source, "");
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