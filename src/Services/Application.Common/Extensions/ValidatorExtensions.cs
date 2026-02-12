using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using FluentValidation;

namespace Application.Common.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, int> CurrencyMustExist<T>(this IRuleBuilder<T, int> ruleBuilder,
        ICurrencyConverter currencyConverter)
    {
        return ruleBuilder
            .Must(currencyConverter.IsSupportedCurrency)
            .WithMessage("Не удалось найти валюту.");
    }
    
    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int?> CurrencyMustExist(ICurrencyConverter currencyConverter)
        {
            return ruleBuilder
                .Must(x => x == null || currencyConverter.IsSupportedCurrency(x!.Value))
                .WithMessage("Не удалось найти валюту.");
        }

        public IRuleBuilderOptions<T, int?> CurrencyMustExistRequired(ICurrencyConverter currencyConverter)
        {
            return ruleBuilder
                .NotNull()
                .WithMessage("Валюта обязательна")
                .Must(x => currencyConverter.IsSupportedCurrency(x!.Value))
                .WithMessage("Не удалось найти валюту.");
        }
    }
}