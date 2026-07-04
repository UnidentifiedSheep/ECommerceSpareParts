using Integrations.Supplier;

namespace Pricing.Application.Static;

public static class CacheKeys
{
    public static class CurrencyCache
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

        public static string CurrencyRate(int currencyId) { return $"currency:{currencyId}:rate"; }
    }

    public static class PricingCache
    {
        public static class Lock
        {
            public static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
            public static string SupplierRequest(int productId, Supplier supplier)
            {
                return $"supplier:{supplier.ToString()}:pricing:request:product:{productId}:lock";
            }
        }
    }
}