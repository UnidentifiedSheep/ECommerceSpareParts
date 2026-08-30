using System.Text.Json;

namespace Domain.Validation;

public static class ValidationExtensions
{
	public static bool HasMinLength(this string value, int min) => value.Length >= min;

	public static bool HasMaxLength(this string value, int max) => value.Length <= max;

	public static bool HasNoSpaces(this string value) =>
		!value.Contains(' ', StringComparison.InvariantCulture);

	public static bool IsNotNullOrEmpty(this string? value) => !string.IsNullOrEmpty(value);

	public static bool IsNotNullOrWhiteSpace(this string? value) => !string.IsNullOrWhiteSpace(value);

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

	public static bool IsNotNull<T>(this T? value) where T : class => value is not null;

	public static bool IsNotNullOrDefault<T>(this T? value) where T : struct
	{
		return value.HasValue && !EqualityComparer<T>.Default.Equals(value.Value, default);
	}

	public static bool IsNullOrDefault<T>(this T? value) where T : struct => !value.IsNotNullOrDefault();

	public static bool IsInRange<T>(
		this T value,
		T min,
		T max) where T : IComparable<T> => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;

	public static bool IsValid<T>(this T value, Func<T, bool> predicate) => predicate(value);

	public static bool IsTrue(this bool value) => value;

	public static bool IsNotEqual<T>(this T value, T next) where T : IComparable<T> =>
		value.CompareTo(next) != 0;

	public static bool IsAtMost<T>(this T value, T max) where T : IComparable<T> => value.CompareTo(max) <= 0;

	public static bool IsAtLeast<T>(this T value, T min) where T : IComparable<T> =>
		value.CompareTo(min) >= 0;

	public static bool IsGreaterThan<T>(this T value, T min) where T : IComparable<T> =>
		value.CompareTo(min) > 0;

	public static bool IsLessThan<T>(this T value, T max) where T : IComparable<T> =>
		value.CompareTo(max) < 0;

	public static bool IsNonNegative<T>(this T value) where T : struct, IComparable<T> =>
		value.CompareTo(default) >= 0;

	public static bool IsNonPositive<T>(this T value) where T : struct, IComparable<T> =>
		value.CompareTo(default) <= 0;

	public static bool HasAtMostDecimalPlaces(this decimal value, int maxDecimals)
	{
		var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];

		return decimalPlaces <= maxDecimals;
	}

	public static bool IsNotEmpty<T>(this IEnumerable<T> value) => value.Any();
}
