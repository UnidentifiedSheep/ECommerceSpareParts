using Abstractions.Interfaces.Currency;
using Application.Common.Extensions;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Sales.BaseValidators;

namespace Main.Application.Handlers.Sales.EditSale;

public class EditSaleValidation : AbstractValidator<EditSaleCommand>
{
    public EditSaleValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.Comment)
            .MaximumLength(256)
            .WithLocalizationKey("sale.comment.max");

        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);

        RuleFor(x => x.EditedContent)
            .SetValidator(new EditSaleContentsValidator());
    }
}