using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Interfaces.Cache;

public interface IPriceApplierProvider
{
    Task<PriceApplierConfigurationSnapshot> GetConfigurationAsync(
        CancellationToken ct = default);

    Task InvalidateConfigurationAsync(
        CancellationToken ct = default);
}
