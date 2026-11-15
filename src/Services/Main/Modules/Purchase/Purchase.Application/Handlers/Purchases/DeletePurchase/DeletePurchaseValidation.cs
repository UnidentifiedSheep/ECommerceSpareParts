using Application.Common.Extensions;
using Main.Entities.Purchase;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

public class DeletePurchaseValidation : AbstractValidator<DeletePurchaseCommand>
{
	public DeletePurchaseValidation()
		=> RuleFor(x => x.PurchaseId).NotEmpty().WithLocalizableError(PurchaseIdNotEmptyMessage.Instance);
}
