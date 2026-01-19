using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;
using Main.Application.Handlers.Sales.BaseValidators;
using Main.Application.Handlers.Sales.BaseValidators.Edit;

namespace Main.Application.Handlers.Sales.EditSale;

public class EditSaleValidation : AbstractValidator<EditSaleCommand>
{
    public EditSaleValidation(ICurrencyConverter currencyConverter)
    {
        RuleFor(x => x.Comment)
            .Must(x => x?.Trim().Length <= 256)
            .WithMessage("Максимальная длина общего комментария — 256 символов.");


        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.CurrencyId)
            .CurrencyMustExist(currencyConverter);
        
        RuleFor(x => x.EditedContent).SetValidator(new SaleContentValidator());
    }
}