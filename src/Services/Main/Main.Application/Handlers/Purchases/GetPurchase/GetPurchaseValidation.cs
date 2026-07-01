using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Purchases.GetPurchase;

public class GetPurchaseValidation : AbstractValidator<GetPurchaseQuery>
{
    public GetPurchaseValidation()
    {
        RuleFor(x => x)
            .Must(x => x.PurchaseId.HasValue || x.TransactionId.HasValue)
            .WithLocalizationKey("purchase.id.or.transaction.id.required");
    }
}