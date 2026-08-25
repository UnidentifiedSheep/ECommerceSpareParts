using Pricing.Application.Models;

namespace Pricing.Application.Interfaces.Pricing;

public interface ISupplierOfferExtractorService
{
    Task<SupplierOfferExtractionResult[]> ExtractOffers(
        string storageCode,
        int productId,
        CancellationToken token = default);
}