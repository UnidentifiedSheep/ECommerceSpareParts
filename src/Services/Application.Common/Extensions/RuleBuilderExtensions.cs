using FluentValidation;
using Locan.Core.Interfaces;

namespace Application.Common.Extensions;

public static class RuleBuilderExtensions
{
	public static IRuleBuilderOptions<T, TProperty> WithLocalizableError<T, TProperty>(
		this IRuleBuilderOptions<T, TProperty> rule,
		ILocalizableMessage message)
	{
		ArgumentNullException.ThrowIfNull(message);
		var configurable = DefaultValidatorOptions.Configurable(rule);
		configurable.Current.ErrorCode = message.MessageKey;
		configurable.Current.CustomStateProvider = (_, _) => message;
		return rule;
	}

	public static IRuleBuilderOptions<T, TProperty> WithLocalizableError<T, TProperty>(
		this IRuleBuilderOptions<T, TProperty> rule,
		Func<TProperty, ILocalizableMessage> message,
		string errorCode)
	{
		ArgumentNullException.ThrowIfNull(message);

		var configurable = DefaultValidatorOptions.Configurable(rule);
		configurable.Current.CustomStateProvider = (_, propertyValue) => message(propertyValue);
		configurable.Current.ErrorCode = errorCode;

		return rule;
	}
}
