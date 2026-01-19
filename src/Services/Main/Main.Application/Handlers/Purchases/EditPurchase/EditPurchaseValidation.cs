using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;
using Main.Application.Handlers.Purchases.BaseValidators;
using Main.Application.Handlers.Purchases.BaseValidators.Edit;

namespace Main.Application.Handlers.Purchases.EditPurchase;

public class EditPurchaseValidation : AbstractValidator<EditPurchaseCommand>
{
    public EditPurchaseValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty().WithMessage("Id закупки не может быть пуст");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Содержимое закупки не может быть пустым");

        RuleFor(x => x.PurchaseDateTime)
            .SetValidator(new PurchaseDateTimeValidator());

        RuleFor(x => x.Content)
            .SetValidator(new EditPurchaseDtoValidation());

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
    }
}