using FluentValidation;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

public class DeletePurchaseValidation : AbstractValidator<DeletePurchaseCommand>
{
    public DeletePurchaseValidation()
    {
        RuleFor(x => x.PurchaseId).NotEmpty().WithMessage("Айди закупки не должен быть пустым");
    }
}