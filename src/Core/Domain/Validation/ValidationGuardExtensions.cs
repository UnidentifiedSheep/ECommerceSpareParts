using Exceptions;

namespace Domain.Extensions;

public static class ValidationGuardExtensions
{
    public static string EnsureMinLength(
        this string value,
        int min,
        string errorKey)
    {
        return !value.HasMinLength(min) ? throw new InvalidInputException(errorKey) : value;
    }

    public static string EnsureMaxLength(
        this string value,
        int max,
        string errorKey)
    {
        return !value.HasMaxLength(max) ? throw new InvalidInputException(errorKey) : value;
    }

    public static string EnsureNoSpaces(this string value, string errorKey)
    {
        return !value.HasNoSpaces() ? throw new InvalidInputException(errorKey) : value;
    }

    public static string EnsureNotNullOrEmpty(this string value, string errorKey)
    {
        return !value.IsNotNullOrEmpty() ? throw new InvalidInputException(errorKey) : value;
    }

    public static string EnsureNotNullOrWhiteSpace(this string value, string errorKey)
    {
        return !value.IsNotNullOrWhiteSpace() ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureNotNull<T>(this T value, string errorKey)
        where T : class
    {
        return !value.IsNotNull() ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureInRange<T>(
        this T value,
        T min,
        T max,
        string errorKey)
        where T : IComparable<T>
    {
        return !value.IsInRange(min, max)
            ? throw new InvalidInputException(errorKey)
            : value;
    }

    public static T Ensure<T>(
        this T value,
        Func<T, bool> predicate,
        string errorKey)
    {
        return !value.IsValid(predicate)
            ? throw new InvalidInputException(errorKey)
            : value;
    }

    public static T EnsureNotEqual<T>(
        this T value,
        T next,
        string errorKey)
        where T : IComparable<T>
    {
        return !value.IsNotEqual(next) ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureAtMost<T>(
        this T value,
        T max,
        string errorKey)
        where T : IComparable<T>
    {
        return !value.IsAtMost(max) ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureAtLeast<T>(
        this T value,
        T min,
        string errorKey)
        where T : IComparable<T>
    {
        return !value.IsAtLeast(min) ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureGreaterThan<T>(
        this T value,
        T min,
        string errorKey)
        where T : IComparable<T>
    {
        return !value.IsGreaterThan(min) ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureLessThan<T>(
        this T value,
        T max,
        string errorKey)
        where T : IComparable<T>
    {
        return !value.IsLessThan(max) ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureNonNegative<T>(this T value, string errorKey)
        where T : struct, IComparable<T>
    {
        return !value.IsNonNegative() ? throw new InvalidInputException(errorKey) : value;
    }

    public static T EnsureNonPositive<T>(this T value, string errorKey)
        where T : struct, IComparable<T>
    {
        return !value.IsNonPositive() ? throw new InvalidInputException(errorKey) : value;
    }

    public static decimal EnsureMaxDecimalPlaces(
        this decimal value,
        int maxDecimals,
        string errorKey)
    {
        return !value.HasAtMostDecimalPlaces(maxDecimals)
            ? throw new InvalidInputException(errorKey)
            : value;
    }

    public static IEnumerable<T> EnsureNotEmpty<T>(this IEnumerable<T> value, string errorKey)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        return !value.IsNotEmpty() ? throw new InvalidInputException(errorKey) : value;
    }

    public static string EnsureMinLength(
        this string value,
        int min,
        Func<Exception> exceptionFactory)
    {
        return !value.HasMinLength(min) ? throw exceptionFactory() : value;
    }

    public static string EnsureMaxLength(
        this string value,
        int max,
        Func<Exception> exceptionFactory)
    {
        return !value.HasMaxLength(max) ? throw exceptionFactory() : value;
    }

    public static string EnsureNoSpaces(this string value, Func<Exception> exceptionFactory)
    {
        return !value.HasNoSpaces() ? throw exceptionFactory() : value;
    }

    public static string EnsureNotNullOrEmpty(this string value, Func<Exception> exceptionFactory)
    {
        return !value.IsNotNullOrEmpty() ? throw exceptionFactory() : value;
    }

    public static string EnsureNotNullOrWhiteSpace(this string value, Func<Exception> exceptionFactory)
    {
        return !value.IsNotNullOrWhiteSpace() ? throw exceptionFactory() : value;
    }

    public static T EnsureNotNull<T>(this T value, Func<Exception> exceptionFactory)
        where T : class
    {
        return !value.IsNotNull() ? throw exceptionFactory() : value;
    }

    public static T EnsureInRange<T>(
        this T value,
        T min,
        T max,
        Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return !value.IsInRange(min, max)
            ? throw exceptionFactory()
            : value;
    }

    public static T Ensure<T>(
        this T value,
        Func<T, bool> predicate,
        Func<Exception> exceptionFactory)
    {
        return !value.IsValid(predicate)
            ? throw exceptionFactory()
            : value;
    }

    public static T EnsureNotEqual<T>(
        this T value,
        T next,
        Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return !value.IsNotEqual(next) ? throw exceptionFactory() : value;
    }

    public static T EnsureAtMost<T>(
        this T value,
        T max,
        Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return !value.IsAtMost(max) ? throw exceptionFactory() : value;
    }

    public static T EnsureAtLeast<T>(
        this T value,
        T min,
        Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return !value.IsAtLeast(min) ? throw exceptionFactory() : value;
    }

    public static T EnsureGreaterThan<T>(
        this T value,
        T min,
        Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return !value.IsGreaterThan(min) ? throw exceptionFactory() : value;
    }

    public static T EnsureLessThan<T>(
        this T value,
        T max,
        Func<Exception> exceptionFactory)
        where T : IComparable<T>
    {
        return !value.IsLessThan(max) ? throw exceptionFactory() : value;
    }


    public static T EnsureNonNegative<T>(this T value, Func<Exception> exceptionFactory)
        where T : struct, IComparable<T>
    {
        return !value.IsNonNegative() ? throw exceptionFactory() : value;
    }

    public static T EnsureNonPositive<T>(this T value, Func<Exception> exceptionFactory)
        where T : struct, IComparable<T>
    {
        return !value.IsNonPositive() ? throw exceptionFactory() : value;
    }

    public static decimal EnsureMaxDecimalPlaces(
        this decimal value,
        int maxDecimals,
        Func<Exception> exceptionFactory)
    {
        return !value.HasAtMostDecimalPlaces(maxDecimals)
            ? throw exceptionFactory()
            : value;
    }

    public static IEnumerable<T> EnsureNotEmpty<T>(this IEnumerable<T> value, Func<Exception> exceptionFactory)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        return !value.IsNotEmpty() ? throw exceptionFactory() : value;
    }
}
