using Exceptions;

namespace Domain.Extensions;

public static class ValidationGuardExtensions
{
    public static string AgainstTooShort(this string value, int min, string errorKey)
    {
        return value.Length < min ? throw new InvalidInputException(errorKey) : value;
    }

    public static string AgainstTooLong(this string value, int max, string errorKey)
    {
        return value.Length > max ? throw new InvalidInputException(errorKey) : value;
    }

    public static string AgainstSpaces(this string value, string errorKey)
    {
        return value.Contains(' ') ? throw new InvalidInputException(errorKey) : value;
    }
    
    public static string AgainstNullOrEmpty(this string value, string errorKey)
    {
        return string.IsNullOrEmpty(value) ? throw new InvalidInputException(errorKey) : value;
    }

    public static string AgainstNullOrWhiteSpace(this string value, string errorKey)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new InvalidInputException(errorKey) : value;
    }

    public static T AgainstNull<T>(this T value, string errorKey)
        where T : class
    {
        return value is null ? throw new InvalidInputException(errorKey) : value;
    }
    
    public static T AgainstOutOfRange<T>(this T value, T min, T max, string errorKey)
        where T : IComparable<T>
    {
        return value.CompareTo(min) < 0 || value.CompareTo(max) > 0
            ? throw new InvalidInputException(errorKey)
            : value;
    }
    
    public static T Against<T>(this T value, Func<T, bool> predicate, string errorKey)
    {
        return predicate(value)
            ? throw new InvalidInputException(errorKey)
            : value;
    }

    public static T AgainstEqual<T>(this T value, T next, string errorKey)
        where T : IComparable<T>
    {
        return value.CompareTo(next) == 0 ? throw new InvalidInputException(errorKey) : value;
    }

    public static T AgainstTooBig<T>(this T value, T max, string errorKey)
        where T : IComparable<T>
    {
        return value.CompareTo(max) > 0 ? throw new InvalidInputException(errorKey) : value;
    }

    public static T AgainstTooSmall<T>(this T value, T min, string errorKey)
        where T : IComparable<T>
    {
        return value.CompareTo(min) < 0 ? throw new InvalidInputException(errorKey) : value;
    }
    
    public static T AgainstLessOrEqual<T>(this T value, T min, string errorKey)
        where T : IComparable<T>
    {
        return value.CompareTo(min) <= 0 ? throw new InvalidInputException(errorKey) : value;
    }

    public static T AgainstGreaterOrEqual<T>(this T value, T max, string errorKey)
        where T : IComparable<T>
    {
        return value.CompareTo(max) >= 0 ? throw new InvalidInputException(errorKey) : value;
    }

    public static T AgainstNegative<T>(this T value, string errorKey)
        where T : struct, IComparable<T>
    {
        return value.CompareTo(default) < 0 ? throw new InvalidInputException(errorKey) : value;
    }
    
    public static decimal AgainstTooManyDecimalPlaces(this decimal value, int maxDecimals, string errorKey)
    {
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
    
        return decimalPlaces > maxDecimals
            ? throw new InvalidInputException(errorKey)
            : value;
    }
    
    public static IEnumerable<T> AgainstEmpty<T>(this IEnumerable<T> value, string errorKey)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        return !value.Any() ? throw new  InvalidInputException(errorKey) : value;
    }
    
    public static string AgainstTooShort(this string value, int min, Func<Exception> exceptionFactory)
    {
        return value.Length < min ? throw exceptionFactory() : value;
    }

    public static string AgainstTooLong(this string value, int max, Func<Exception> exceptionFactory)
    {
        return value.Length > max ? throw exceptionFactory() : value;
    }

    public static string AgainstSpaces(this string value, Func<Exception> exceptionFactory)
    {
        return value.Contains(' ') ? throw exceptionFactory() : value;
    }
    
    public static string AgainstNullOrEmpty(this string value, Func<Exception> exceptionFactory)
    {
        return string.IsNullOrEmpty(value) ? throw exceptionFactory() : value;
    }

    public static string AgainstNullOrWhiteSpace(this string value, Func<Exception> exceptionFactory)
    {
        return string.IsNullOrWhiteSpace(value) ? throw exceptionFactory() : value;
    }

    public static T AgainstNull<T>(this T value, Func<Exception> exceptionFactory)
        where T : class
    {
        return value is null ? throw exceptionFactory() : value;
    }
    
    public static T AgainstOutOfRange<T>(this T value, T min, T max, Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return value.CompareTo(min) < 0 || value.CompareTo(max) > 0
            ? throw exceptionFactory()
            : value;
    }
    
    public static T Against<T>(this T value, Func<T, bool> predicate, Func<Exception> exceptionFactory)
    {
        return predicate(value)
            ? throw exceptionFactory()
            : value;
    }

    public static T AgainstEqual<T>(this T value, T next, Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return value.CompareTo(next) == 0 ? throw exceptionFactory() : value;
    }

    public static T AgainstTooBig<T>(this T value, T max, Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return value.CompareTo(max) > 0 ? throw exceptionFactory() : value;
    }

    public static T AgainstTooSmall<T>(this T value, T min, Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return value.CompareTo(min) < 0 ? throw exceptionFactory() : value;
    }
    
    public static T AgainstLessOrEqual<T>(this T value, T min, Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return value.CompareTo(min) <= 0 ? throw exceptionFactory() : value;
    }

    public static T AgainstGreaterOrEqual<T>(this T value, T max, Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return value.CompareTo(max) >= 0 ? throw exceptionFactory() : value;
    }
    

    public static T AgainstNegative<T>(this T value, Func<Exception> exceptionFactory)
        where T : struct, IComparable<T>
    {
        return value.CompareTo(default) < 0 ? throw exceptionFactory() : value;
    }
    
    public static decimal AgainstTooManyDecimalPlaces(this decimal value, int maxDecimals, Func<Exception> exceptionFactory)
    {
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
    
        return decimalPlaces > maxDecimals
            ? throw exceptionFactory()
            : value;
    }
    
    public static IEnumerable<T> AgainstEmpty<T>(this IEnumerable<T> value, Func<Exception> exceptionFactory)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        return !value.Any() ? throw exceptionFactory() : value;
    }
}