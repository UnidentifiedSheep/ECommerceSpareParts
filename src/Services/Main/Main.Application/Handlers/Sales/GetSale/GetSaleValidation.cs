using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Sales.GetSale;

public class GetSaleValidation : AbstractValidator<GetSaleQuery>
{
    public GetSaleValidation()
    {
        RuleFor(x => x)
            .Must(x => x.SaleId.HasValue || x.TransactionId.HasValue)
            .WithLocalizationKey("sale.id.or.transaction.id.required");
    }
}
