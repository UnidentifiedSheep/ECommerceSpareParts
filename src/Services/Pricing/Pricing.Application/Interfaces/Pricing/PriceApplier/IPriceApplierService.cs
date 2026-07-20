using Pricing.Enums;

namespace Pricing.Application.Interfaces.Pricing.PriceApplier;

public interface IPriceApplierService
{
    Task<IReadOnlyList<IPriceApplier>> GetPriceAppliersAsync(
        PriceOfferSourceType usage,
        CancellationToken ct = default);
}