using Application.Handlers.Sales.BaseValidators;
using Application.Handlers.Sales.BaseValidators.Create;
using FluentValidation;

namespace Application.Handlers.Sales.CreateFullSale;

public class CreateFullSaleValidation : AbstractValidator<CreateFullSaleCommand>
{
    public CreateFullSaleValidation()
    {
        RuleFor(x => x.SaleDateTime)
            .SetValidator(new SaleDateTimeValidator());
        
        RuleFor(x => x.SaleContent)
            .SetValidator(new SaleContentValidator());

        RuleFor(x => x.PayedSum)
            .GreaterThan(0)
            .When(x => x.PayedSum != null)
            .WithMessage("Оплаченная сумма должна быть больше или равна 0, если указана")
            .PrecisionScale(18, 2, true)
            .WithMessage("Оплаченная сумма может содержать не более двух знаков после запятой");
            
        
        RuleFor(x => x.BuyerId).NotEmpty()
            .WithMessage("Id покупателя не может быть пустым");
        
        RuleFor(x => x.CreatedUserId).NotEmpty()
            .WithMessage("Id пользователя создавшего закупку не может быть пустым");
    }
}