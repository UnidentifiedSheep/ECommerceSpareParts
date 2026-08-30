using Application.Common.Interfaces.Cqrs;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Entities.Exceptions;

namespace Pricing.Application.Handlers.PriceApplier;

public record GetPriceApplierQuery(string SystemName) : IQuery<GetPriceApplierResult>;

public record GetPriceApplierResult(PriceApplierDto Applier);

public class GetPriceApplierHandler(IPriceApplierService service)
	: IQueryHandler<GetPriceApplierQuery, GetPriceApplierResult>
{
	public async Task<GetPriceApplierResult> Handle(
		GetPriceApplierQuery request,
		CancellationToken cancellationToken)
	{
		var applier = await service.FindPriceApplierInfoAsync(request.SystemName, cancellationToken) ??
			throw new PriceApplierNotFoundException(request.SystemName);

		return new GetPriceApplierResult(applier);
	}
}
