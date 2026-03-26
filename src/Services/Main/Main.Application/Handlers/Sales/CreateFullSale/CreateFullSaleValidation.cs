using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Sales.BaseValidators;

namespace Main.Application.Handlers.Sales.CreateFullSale;

public class CreateFullSaleValidation : AbstractValidator<CreateFullSaleCommand>
{
    public CreateFullSaleValidation()
    {
        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());

        RuleFor(x => x.SaleContent)
            .SetValidator(new NewSaleContentValidator());

        RuleFor(x => x.PayedSum)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PayedSum != null)
            .WithLocalizationKey("sale.payed.sum.min")
            .PrecisionScale(18, 2, true)
            .When(x => x.PayedSum != null)
            .WithLocalizationKey("sale.payed.sum.precision");

        RuleFor(x => x.BuyerId)
            .NotEmpty()
            .WithLocalizationKey("sale.buyer.id.not.empty");

        RuleFor(x => x.CreatedUserId)
            .NotEmpty()
            .WithLocalizationKey("sale.created.user.id.not.empty");
    }
}