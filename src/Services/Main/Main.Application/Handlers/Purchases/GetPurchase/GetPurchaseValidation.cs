using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.GetPurchase;

public class GetPurchaseValidation : AbstractValidator<GetPurchaseQuery>
{
	public GetPurchaseValidation()
	{
		RuleFor(x => x)
			.Must(x => x.PurchaseId.HasValue || x.TransactionId.HasValue)
			.WithLocalizableError(PurchaseIdOrTransactionIdRequiredMessage.Instance);
	}
}
