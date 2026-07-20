using Application.Common.Interfaces.Cqrs;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Enums;

namespace Pricing.Application.Handlers.PriceApplier.GetPriceAppliers;

public record GetPriceAppliersQuery(
    PriceOfferSourceType Usage
) : IQuery<GetPriceAppliersResult>;

public record GetPriceAppliersResult(
    IReadOnlyList<PriceApplierDto> Appliers);

public class GetPriceAppliersHandler(
    IPriceApplierService service
) : IQueryHandler<GetPriceAppliersQuery, GetPriceAppliersResult>
{
    public async Task<GetPriceAppliersResult> Handle(
        GetPriceAppliersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await service.GetPriceApplierInfosAsync(
            request.Usage,
            cancellationToken);
        return new GetPriceAppliersResult(result);
    }
}
