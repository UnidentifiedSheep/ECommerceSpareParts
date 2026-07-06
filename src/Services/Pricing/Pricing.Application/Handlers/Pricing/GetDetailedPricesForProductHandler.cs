using Application.Common.Interfaces.Cqrs;
using Pricing.Application.Interfaces.Pricing;

namespace Pricing.Application.Handlers.Pricing;

public record GetDetailedPricesForProductQuery(
    int ProductId
    ) : IQuery<GetDetailedPricesForProductResult>;

public record GetDetailedPricesForProductResult();

public class GetDetailedPricesForProductHandler(
    ISupplierOfferExtractorService extractorService
    ) : IQueryHandler<GetDetailedPricesForProductQuery, GetDetailedPricesForProductResult>
{
    public Task<GetDetailedPricesForProductResult> Handle(GetDetailedPricesForProductQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}