using BulkValidation.Core.Interfaces;
using Core.Interfaces;
using FluentValidation;
using Main.Abstractions.Interfaces;

namespace Main.Application.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, int> CurrencyMustExist<T>(this IRuleBuilder<T, int> ruleBuilder,
        ICurrencyConverter currencyConverter)
    {
        return ruleBuilder
            .Must(currencyConverter.IsSupportedCurrency)
            .WithMessage("Не удалось найти валюту.");
    }
    
    public static IRuleBuilderOptions<T, int?> CurrencyMustExist<T>(this IRuleBuilder<T, int?> ruleBuilder,
        ICurrencyConverter currencyConverter)
    {
        return ruleBuilder
            .Must(x => x == null || currencyConverter.IsSupportedCurrency(x!.Value))
            .WithMessage("Не удалось найти валюту.");
    }
    
    public static IRuleBuilderOptions<T, int?> CurrencyMustExistRequired<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        ICurrencyConverter currencyConverter)
    {
        return ruleBuilder
            .NotNull()
            .WithMessage("Валюта обязательна")
            .Must(x => currencyConverter.IsSupportedCurrency(x!.Value))
            .WithMessage("Не удалось найти валюту.");
    }
}