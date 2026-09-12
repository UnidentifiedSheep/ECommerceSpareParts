using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Sales.GetSale;

public class GetSaleValidation : AbstractValidator<GetSaleQuery>
{
	public GetSaleValidation()
	{
		RuleFor(x => x)
			.Must(x => x.SaleId.HasValue || x.TransactionId.HasValue)
			.WithLocalizableError(SaleIdOrTransactionIdRequiredMessage.Instance);
	}
}
