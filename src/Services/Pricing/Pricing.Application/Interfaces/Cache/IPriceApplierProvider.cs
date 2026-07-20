using Pricing.Application.Dtos.PriceApplier;

namespace Pricing.Application.Interfaces.Cache;

public interface IPriceApplierProvider
{
    Task<IReadOnlyList<PriceApplierDto>> GetPriceAppliersAsync(
        CancellationToken ct = default);

    Task InvalidatePriceAppliersAsync(
        CancellationToken ct = default);
}