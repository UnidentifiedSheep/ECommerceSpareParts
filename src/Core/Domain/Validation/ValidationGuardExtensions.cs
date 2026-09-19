using Exceptions;
using Locan.Core.Interfaces;

namespace Domain.Validation;

public static class ValidationGuardExtensions
{
	public static string EnsureMinLength(
		this string value,
		int min,
		ILocalizableMessage message) =>
		!value.HasMinLength(min) ? throw new InvalidInputException(message) : value;

	public static string EnsureMaxLength(
		this string value,
		int max,
		ILocalizableMessage message) =>
		!value.HasMaxLength(max) ? throw new InvalidInputException(message) : value;

	public static string EnsureNoSpaces(this string value, ILocalizableMessage message) =>
		!value.HasNoSpaces() ? throw new InvalidInputException(message) : value;

	public static string EnsureNotNullOrEmpty(this string value, ILocalizableMessage message) =>
		!value.IsNotNullOrEmpty() ? throw new InvalidInputException(message) : value;

	public static string EnsureNotNullOrWhiteSpace(this string value, ILocalizableMessage message) =>
		!value.IsNotNullOrWhiteSpace() ? throw new InvalidInputException(message) : value;

	public static T EnsureNotNull<T>(this T value, ILocalizableMessage message) where T : class =>
		!value.IsNotNull() ? throw new InvalidInputException(message) : value;

	public static T EnsureNotNullOrDefault<T>(this T? value, ILocalizableMessage message) where T : struct
	{
		return !value.IsNotNullOrDefault()
			? throw new InvalidInputException(message)
			: value.GetValueOrDefault();
	}

	public static T EnsureNullOrDefault<T>(this T? value, ILocalizableMessage message) where T : struct
	{
		return !value.IsNullOrDefault()
			? throw new InvalidInputException(message)
			: value.GetValueOrDefault();
	}

	public static T EnsureInRange<T>(
		this T value,
		T min,
		T max,
		ILocalizableMessage message) where T : IComparable<T> =>
		!value.IsInRange(min, max) ? throw new InvalidInputException(message) : value;

	public static T Ensure<T>(
		this T value,
		Func<T, bool> predicate,
		ILocalizableMessage message) =>
		!value.IsValid(predicate) ? throw new InvalidInputException(message) : value;

	public static bool EnsureTrue(this bool value, ILocalizableMessage message) =>
		!value.IsTrue() ? throw new InvalidInputException(message) : value;

	public static T EnsureNotEqual<T>(
		this T value,
		T next,
		ILocalizableMessage message) where T : IComparable<T> =>
		!value.IsNotEqual(next) ? throw new InvalidInputException(message) : value;

	public static T EnsureAtMost<T>(
		this T value,
		T max,
		ILocalizableMessage message) where T : IComparable<T> =>
		!value.IsAtMost(max) ? throw new InvalidInputException(message) : value;

	public static T EnsureAtLeast<T>(
		this T value,
		T min,
		ILocalizableMessage message) where T : IComparable<T> =>
		!value.IsAtLeast(min) ? throw new InvalidInputException(message) : value;

	public static T EnsureGreaterThan<T>(
		this T value,
		T min,
		ILocalizableMessage message) where T : IComparable<T> =>
		!value.IsGreaterThan(min) ? throw new InvalidInputException(message) : value;

	public static T EnsureLessThan<T>(
		this T value,
		T max,
		ILocalizableMessage message) where T : IComparable<T> =>
		!value.IsLessThan(max) ? throw new InvalidInputException(message) : value;

	public static T EnsureNonNegative<T>(this T value, ILocalizableMessage message)
		where T : struct, IComparable<T> =>
		!value.IsNonNegative() ? throw new InvalidInputException(message) : value;

	public static T EnsureNonPositive<T>(this T value, ILocalizableMessage message)
		where T : struct, IComparable<T> =>
		!value.IsNonPositive() ? throw new InvalidInputException(message) : value;

	public static decimal EnsureMaxDecimalPlaces(
		this decimal value,
		int maxDecimals,
		ILocalizableMessage message) => !value.HasAtMostDecimalPlaces(maxDecimals)
		? throw new InvalidInputException(message)
		: value;

	public static IEnumerable<T> EnsureNotEmpty<T>(this IEnumerable<T> value, ILocalizableMessage message)
	{
		// ReSharper disable once PossibleMultipleEnumeration
		return !value.IsNotEmpty() ? throw new InvalidInputException(message) : value;
	}

	public static string EnsureMinLength(
		this string value,
		int min,
		Func<Exception> exceptionFactory) => !value.HasMinLength(min) ? throw exceptionFactory() : value;

	public static string EnsureMaxLength(
		this string value,
		int max,
		Func<Exception> exceptionFactory) => !value.HasMaxLength(max) ? throw exceptionFactory() : value;

	public static string EnsureNoSpaces(this string value, Func<Exception> exceptionFactory) =>
		!value.HasNoSpaces() ? throw exceptionFactory() : value;

	public static string EnsureNotNullOrEmpty(this string value, Func<Exception> exceptionFactory) =>
		!value.IsNotNullOrEmpty() ? throw exceptionFactory() : value;

	public static string EnsureNotNullOrWhiteSpace(this string value, Func<Exception> exceptionFactory) =>
		!value.IsNotNullOrWhiteSpace() ? throw exceptionFactory() : value;

	public static string EnsureValidJson(this string value, Func<Exception> exceptionFactory) =>
		!value.IsValidJson() ? throw exceptionFactory() : value;

	public static T EnsureNotNull<T>(this T value, Func<Exception> exceptionFactory) where T : class =>
		!value.IsNotNull() ? throw exceptionFactory() : value;

	public static T EnsureNotNullOrDefault<T>(this T? value, Func<Exception> exceptionFactory)
		where T : struct =>
		!value.IsNotNullOrDefault() ? throw exceptionFactory() : value.GetValueOrDefault();

	public static T EnsureNullOrDefault<T>(this T? value, Func<Exception> exceptionFactory)
		where T : struct => !value.IsNullOrDefault() ? throw exceptionFactory() : value.GetValueOrDefault();

	public static T EnsureInRange<T>(
		this T value,
		T min,
		T max,
		Func<Exception> exceptionFactory) where T : IComparable<T> =>
		!value.IsInRange(min, max) ? throw exceptionFactory() : value;

	public static T Ensure<T>(
		this T value,
		Func<T, bool> predicate,
		Func<Exception> exceptionFactory) => !value.IsValid(predicate) ? throw exceptionFactory() : value;

	public static bool EnsureTrue(this bool value, Func<Exception> exceptionFactory) =>
		!value.IsTrue() ? throw exceptionFactory() : value;

	public static T EnsureNotEqual<T>(
		this T value,
		T next,
		Func<Exception> exceptionFactory) where T : IComparable<T> =>
		!value.IsNotEqual(next) ? throw exceptionFactory() : value;

	public static T EnsureAtMost<T>(
		this T value,
		T max,
		Func<Exception> exceptionFactory) where T : IComparable<T> =>
		!value.IsAtMost(max) ? throw exceptionFactory() : value;

	public static T EnsureAtLeast<T>(
		this T value,
		T min,
		Func<Exception> exceptionFactory) where T : IComparable<T> =>
		!value.IsAtLeast(min) ? throw exceptionFactory() : value;

	public static T EnsureGreaterThan<T>(
		this T value,
		T min,
		Func<Exception> exceptionFactory) where T : IComparable<T> =>
		!value.IsGreaterThan(min) ? throw exceptionFactory() : value;

	public static T EnsureLessThan<T>(
		this T value,
		T max,
		Func<Exception> exceptionFactory) where T : IComparable<T> =>
		!value.IsLessThan(max) ? throw exceptionFactory() : value;

	public static T EnsureNonNegative<T>(this T value, Func<Exception> exceptionFactory)
		where T : struct, IComparable<T> => !value.IsNonNegative() ? throw exceptionFactory() : value;

	public static T EnsureNonPositive<T>(this T value, Func<Exception> exceptionFactory)
		where T : struct, IComparable<T> => !value.IsNonPositive() ? throw exceptionFactory() : value;

	public static decimal EnsureMaxDecimalPlaces(
		this decimal value,
		int maxDecimals,
		Func<Exception> exceptionFactory) =>
		!value.HasAtMostDecimalPlaces(maxDecimals) ? throw exceptionFactory() : value;

	public static IEnumerable<T> EnsureNotEmpty<T>(
		this IEnumerable<T> value,
		Func<Exception> exceptionFactory)
	{
		// ReSharper disable once PossibleMultipleEnumeration
		return !value.IsNotEmpty() ? throw exceptionFactory() : value;
	}
}
