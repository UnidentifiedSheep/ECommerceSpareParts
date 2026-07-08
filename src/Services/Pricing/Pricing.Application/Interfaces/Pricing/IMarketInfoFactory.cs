using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Application.Interfaces.Pricing;

public interface IMarketInfoFactory
{
    Task<MarketInfo> CreateFromSupplierPrices(
        IReadOnlyCollection<CalculatedPriceCandidate> calculatedSupplierCandidates);
}