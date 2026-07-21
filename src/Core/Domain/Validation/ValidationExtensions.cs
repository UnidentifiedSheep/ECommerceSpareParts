using System.Text.Json;
using System.Text.Json.Nodes;

namespace Domain.Extensions;

public static class ValidationExtensions
{
    public static bool HasMinLength(this string value, int min)
    {
        return value.Length >= min;
    }

    public static bool HasMaxLength(this string value, int max)
    {
        return value.Length <= max;
    }

    public static bool HasNoSpaces(this string value)
    {
        return !value.Contains(' ');
    }

    public static bool IsNotNullOrEmpty(this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static bool IsNotNullOrWhiteSpace(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static bool IsValidJson(this string value)
    {
        try
        {
            using var _ = JsonDocument.Parse(value);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static bool IsNotNull<T>(this T? value)
        where T : class
    {
        return value is not null;
    }

    public static bool IsInRange<T>(this T value, T min, T max)
        where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    public static bool IsValid<T>(this T value, Func<T, bool> predicate)
    {
        return predicate(value);
    }

    public static bool IsNotEqual<T>(this T value, T next)
        where T : IComparable<T>
    {
        return value.CompareTo(next) != 0;
    }

    public static bool IsAtMost<T>(this T value, T max)
        where T : IComparable<T>
    {
        return value.CompareTo(max) <= 0;
    }

    public static bool IsAtLeast<T>(this T value, T min)
        where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0;
    }

    public static bool IsGreaterThan<T>(this T value, T min)
        where T : IComparable<T>
    {
        return value.CompareTo(min) > 0;
    }

    public static bool IsLessThan<T>(this T value, T max)
        where T : IComparable<T>
    {
        return value.CompareTo(max) < 0;
    }

    public static bool IsNonNegative<T>(this T value)
        where T : struct, IComparable<T>
    {
        return value.CompareTo(default) >= 0;
    }

    public static bool IsNonPositive<T>(this T value)
        where T : struct, IComparable<T>
    {
        return value.CompareTo(default) <= 0;
    }

    public static bool HasAtMostDecimalPlaces(this decimal value, int maxDecimals)
    {
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];

        return decimalPlaces <= maxDecimals;
    }

    public static bool IsNotEmpty<T>(this IEnumerable<T> value)
    {
        return value.Any();
    }
}
