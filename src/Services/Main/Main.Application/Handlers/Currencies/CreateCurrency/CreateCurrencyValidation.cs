using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

public class CreateCurrencyValidation : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidation()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithLocalizationKey("currency.code.not.empty")
            .MaximumLength(26)
            .WithLocalizationKey("currency.code.max.length")
            .Must(x => x.Trim().Length >= 2)
            .WithLocalizationKey("currency.code.min.length");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithLocalizationKey("currency.name.not.empty")
            .MaximumLength(128)
            .WithLocalizationKey("currency.name.max.length")
            .Must(x => x.Trim().Length >= 3)
            .WithLocalizationKey("currency.name.min.length");

        RuleFor(x => x.CurrencySign)
            .NotEmpty()
            .WithLocalizationKey("currency.sign.not.empty")
            .MaximumLength(3)
            .WithLocalizationKey("currency.sign.max.length")
            .Must(x => x.Trim().Length >= 1)
            .WithLocalizationKey("currency.sign.min.length");

        RuleFor(x => x.ShortName)
            .NotEmpty()
            .WithLocalizationKey("currency.shortName.not.empty")
            .MaximumLength(5)
            .WithLocalizationKey("currency.shortName.max.length")
            .Must(x => x.Trim().Length >= 2)
            .WithLocalizationKey("currency.shortName.min.length");
    }
}