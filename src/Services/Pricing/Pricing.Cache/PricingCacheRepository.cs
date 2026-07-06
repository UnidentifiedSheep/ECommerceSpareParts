using Application.Common.Interfaces.Cache;
using Cache;
using Cache.Extensions;
using Enums;
using Integrations.Supplier;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Static;

namespace Pricing.Cache;

public class PricingCacheRepository(
    ICache cache
) : IPricingCacheRepository
{
}