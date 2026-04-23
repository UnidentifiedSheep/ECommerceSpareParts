using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

public class RestoreContentValidation : AbstractValidator<RestoreContentCommand>
{
    public RestoreContentValidation(ICurrencyConverter currencyConverter)
    {
        RuleForEach(z => z.ContentDetails)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Detail.Count)
                    .SetValidator(new CountValidator());

                z.RuleFor(x => x.Detail.BuyPrice)
                    .SetValidator(new PriceValidator());

                z.RuleFor(x => x.Detail.CurrencyId)
                    .CurrencyMustExist(currencyConverter);
            });

        RuleFor(x => x.ContentDetails)
            .NotEmpty()
            .WithLocalizationKey("storage.content.restore.list.not.empty");
    }
}