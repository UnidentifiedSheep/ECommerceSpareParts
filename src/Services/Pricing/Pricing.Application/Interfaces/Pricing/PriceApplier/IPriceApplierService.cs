using Pricing.Application.Dtos.PriceApplier;
using Pricing.Enums;

namespace Pricing.Application.Interfaces.Pricing.PriceApplier;

public interface IPriceApplierService
{
    Task<IReadOnlyList<PriceApplierDto>> GetPriceApplierInfosAsync(
        PriceOfferSourceType usage,
        CancellationToken ct = default);

    Task<IReadOnlyList<IPriceApplier>> GetPriceAppliersAsync(
        PriceOfferSourceType usage,
        CancellationToken ct = default);
}
