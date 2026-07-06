using Application.Common.Interfaces.Cqrs;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

public record GetDetailedPricesForProductQuery(
    int ProductId) : IQuery<GetDetailedPricesForProductResult>;

public record GetDetailedPricesForProductResult();

public class GetDetailedPricesForProductHandler(
    ISupplierOfferExtractorService extractorService,
    IPriceOfferRepository offerRepository
    ) : IQueryHandler<GetDetailedPricesForProductQuery, GetDetailedPricesForProductResult>
{
    public async Task<GetDetailedPricesForProductResult> Handle(GetDetailedPricesForProductQuery request, CancellationToken cancellationToken)
    {
        var extracted = await extractorService
            .ExtractOffers(
                request.ProductId, 
                cancellationToken);
        
        var offers = new List<PriceOffer>();
        foreach (var result in extracted
                     .Where(x => x is { IsSuccess: true, Offer: not null }))
        {
            foreach (var supplierOffer in result.Offer!.Positions)
            {
                if (supplierOffer.PurchaseInfo == null) continue;
                if (supplierOffer.DeliveryInfo == null) continue;
                
                offers.Add(PriceOffer.Create(
                    request.ProductId,
                    -1, //TODO should be resolved from service CODE -> ID
                    supplierOffer.PurchaseInfo.PriceInfo.Price,
                    result.Supplier.GetFromSupplier(),
                    supplierOffer.Id,
                    supplierOffer.PurchaseInfo.AvailableQuantity,
                    supplierOffer.PurchaseInfo.MinimumPurchaseQuantity,
                    supplierOffer.PurchaseInfo.QuantityCoefficient,
                    supplierOffer.PurchaseInfo.DaysToRefund,
                    supplierOffer.DeliveryInfo.DeliveryDate,
                    supplierOffer.DeliveryInfo.GuaranteedDeliveryDate,
                    supplierOffer.DeliveryInfo.DeliveryProbability,
                    supplierOffer.DeliveryInfo.OrderTill,
                    DateTime.UtcNow.AddDays(1))); //TODO this should be taken from settings, cuz depends on supplier
            }
        }
        
        await offerRepository.UpsertOffersAsync(offers, cancellationToken);

        return new GetDetailedPricesForProductResult();
    }
}