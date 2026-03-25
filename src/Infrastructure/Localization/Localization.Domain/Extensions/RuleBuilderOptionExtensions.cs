using FluentValidation;

namespace Localization.Domain.Extensions;

public static class RuleBuilderOptionExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithLocalizationKey<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        string localizationKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localizationKey);
        DefaultValidatorOptions.Configurable(rule).Current.ErrorCode = localizationKey;
        return rule;
    }
}