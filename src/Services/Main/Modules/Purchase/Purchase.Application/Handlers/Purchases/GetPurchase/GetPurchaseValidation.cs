using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Purchase;

namespace Main.Modules.Purchase.Application.Handlers.Purchases.GetPurchase;

public class GetPurchaseValidation : AbstractValidator<GetPurchaseQuery>
{
	public GetPurchaseValidation()
	{
		RuleFor(x => x)
			.Must(x => x.PurchaseId.HasValue || x.TransactionId.HasValue)
			.WithLocalizableError(PurchaseIdOrTransactionIdRequiredMessage.Instance);
	}
}
