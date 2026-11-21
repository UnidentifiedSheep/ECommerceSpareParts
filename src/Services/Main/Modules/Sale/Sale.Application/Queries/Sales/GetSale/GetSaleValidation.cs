using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Sale;

namespace Main.Modules.Sale.Application.Queries.Sales.GetSale;

public class GetSaleValidation : AbstractValidator<GetSaleQuery>
{
	public GetSaleValidation()
	{
		RuleFor(x => x)
			.Must(x => x.SaleId.HasValue || x.TransactionId.HasValue)
			.WithLocalizableError(SaleIdOrTransactionIdRequiredMessage.Instance);
	}
}
