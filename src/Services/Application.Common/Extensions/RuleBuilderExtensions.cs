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
		DefaultValidatorOptions.Configurable(rule).Current.ErrorCode = message.MessageKey;
		DefaultValidatorOptions.Configurable(rule).Current.CustomStateProvider = (_, _) => message;
		return rule;
	}
}
