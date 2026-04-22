using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.AddContent;

public class AddContentValidation : AbstractValidator<AddContentCommand>
{
    public AddContentValidation(ICurrencyConverter currencyConverter)
    {
        RuleForEach(x => x.StorageContent).ChildRules(content =>
        {
            content.RuleFor(x => x.BuyPrice)
                .SetValidator(new PriceValidator());

            content.RuleFor(x => x.Count)
                .SetValidator(new CountValidator());

            content.RuleFor(x => x.PurchaseDate.ToUniversalTime())
                .InclusiveBetween(DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddMinutes(10))
                .WithLocalizationKey("storage.content.purchase.date.range");

            content.RuleFor(x => x.CurrencyId)
                .CurrencyMustExist(currencyConverter);
        });

        RuleFor(x => x.StorageContent)
            .NotEmpty()
            .WithLocalizationKey("storage.content.list.not.empty");

        RuleFor(x => x.StorageName)
            .NotEmpty()
            .WithLocalizationKey("storage.name.not.empty");
    }
}