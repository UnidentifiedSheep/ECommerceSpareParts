using Application.Common.NamedObject;
using FluentValidation;
using Locan.Core.Interfaces;
using Main.Entities;

namespace Application.Common.Extensions;

public static class SettingRuleBuilderExtensions
{
	public static IRuleBuilderOptions<T, string?> Required<T>(this IRuleBuilder<T, string?> rule)
		=> rule
			.Must(value => !string.IsNullOrWhiteSpace(value))
			.WithFieldError(SettingFieldRequiredMessage.Create);

	public static IRuleBuilderOptions<T, string?> NotEmptyWhenProvided<T>(this IRuleBuilder<T, string?> rule)
		=> rule
			.Must(value => value is null || !string.IsNullOrWhiteSpace(value))
			.WithFieldError(SettingFieldEmptyMessage.Create);

	public static IRuleBuilderOptions<T, string?> MaximumTrimmedLength<T>(
		this IRuleBuilder<T, string?> rule,
		int maxLength)
		=> rule
			.Must(value => value is null || value.Trim().Length <= maxLength)
			.WithFieldError(field => SettingFieldMaxLengthMessage.Create(field, maxLength));

	public static IRuleBuilderOptions<T, string?> HttpUrl<T>(this IRuleBuilder<T, string?> rule)
		=> rule
			.Must(value => value is null || SettingValues.TryParseUrl(value, out _))
			.WithFieldError(SettingFieldUrlInvalidMessage.Create);

	public static IRuleBuilderOptions<T, TProperty> GreaterThanZero<T, TProperty>(this IRuleBuilder<T, TProperty> rule)
		where TProperty : struct, IComparable<TProperty>
		=> rule
			.Must(value => value.CompareTo(default) > 0)
			.WithFieldError(SettingFieldPositiveMessage.Create);

	public static IRuleBuilderOptions<T, TProperty?> GreaterThanZero<T, TProperty>(
		this IRuleBuilder<T, TProperty?> rule)
		where TProperty : struct, IComparable<TProperty>
		=> rule
			.Must(value => value is null || value.Value.CompareTo(default) > 0)
			.WithFieldError(SettingFieldPositiveMessage.Create);

	public static IRuleBuilderOptions<T, TProperty> NotNegative<T, TProperty>(this IRuleBuilder<T, TProperty> rule)
		where TProperty : struct, IComparable<TProperty>
		=> rule
			.Must(value => value.CompareTo(default) >= 0)
			.WithFieldError(SettingFieldNotNegativeMessage.Create);

	public static IRuleBuilderOptions<T, TEnum> DefinedEnum<T, TEnum>(this IRuleBuilder<T, TEnum> rule)
		where TEnum : struct, Enum
		=> rule
			.Must(Enum.IsDefined)
			.WithFieldError(SettingFieldValueInvalidMessage.Create);

	public static IRuleBuilderOptions<T, IReadOnlyList<TItem>?> UniqueBy<T, TItem, TKey>(
		this IRuleBuilder<T, IReadOnlyList<TItem>?> rule,
		Func<TItem, TKey> keySelector,
		IEqualityComparer<TKey>? comparer = null)
		=> rule
			.Must(items => items is null || items.Select(keySelector).Distinct(comparer).Count() == items.Count)
			.WithFieldError(SettingFieldDuplicateMessage.Create);

	public static IRuleBuilderOptions<T, TProperty> WithFieldError<T, TProperty>(
		this IRuleBuilderOptions<T, TProperty> rule,
		Func<string, ILocalizableMessage> message)
	{
		var configurable = DefaultValidatorOptions.Configurable(rule);
		configurable.Current.ErrorCode = message(string.Empty).MessageKey;
		configurable.Current.CustomStateProvider = (context, _) => message(ToFieldName(context.PropertyPath));
		return rule;
	}

	private static string ToFieldName(string propertyPath)
		=> string.Join(
			'.',
			propertyPath
				.Split('.')
				.Select(segment => segment.Length == 0
					? segment
					: char.ToLowerInvariant(segment[0]) + segment[1..]));
}
