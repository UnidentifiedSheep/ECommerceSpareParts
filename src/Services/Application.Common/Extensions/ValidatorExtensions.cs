using Abstractions.Interfaces.Currency;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Application.Common.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, int> CurrencyMustExist<T>(this IRuleBuilder<T, int> ruleBuilder,
        ICurrencyConverter currencyConverter)
    {
        return ruleBuilder
            .Must(currencyConverter.IsSupportedCurrency)
            .WithLocalizationKey("currency.not.found");
    }
    
    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int?> CurrencyMustExist(ICurrencyConverter currencyConverter)
        {
            return ruleBuilder
                .Must(x => x == null || currencyConverter.IsSupportedCurrency(x.Value))
                .WithLocalizationKey("currency.must.exist");
        }

        public IRuleBuilderOptions<T, int?> CurrencyMustExistRequired(ICurrencyConverter currencyConverter)
        {
            return ruleBuilder
                .NotNull()
                .WithLocalizationKey("currency.must.exist")
                .Must(x => currencyConverter.IsSupportedCurrency(x!.Value))
                .WithLocalizationKey("currency.not.found");
        }
    }
}