using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Purchases.BaseValidators;

namespace Main.Application.Handlers.Purchases.EditPurchase;

public class EditPurchaseValidation : AbstractValidator<EditPurchaseCommand>
{
    public EditPurchaseValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty()
            .WithLocalizationKey("purchase.id.not.empty");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithLocalizationKey("purchase.content.not.empty");

        RuleFor(x => x.PurchaseDateTime)
            .SetValidator(new PurchaseDateTimeValidator());

        RuleFor(x => x.Content)
            .SetValidator(new EditPurchaseDtoValidation());

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}