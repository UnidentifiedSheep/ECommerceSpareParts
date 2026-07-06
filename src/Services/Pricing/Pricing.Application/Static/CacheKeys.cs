using Enums;
using Integrations.Supplier;
using Pricing.Entities.Settings;

namespace Pricing.Application.Static;

public static class CacheKeys
{
    public static class Currency
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

        public static string CurrencyRate(int currencyId) { return $"currency:{currencyId}:rate"; }
    }

    public static class Offer
    {
        public static class Ok
        {
            public static TimeSpan Ttl(PricingSettingData setting) => setting.OfferTtl ?? TimeSpan.FromDays(1);
            
            public static string Key(Supplier supplier, int productId)
                => $"offer:refresh:ok:{supplier.ToString()}:{productId}";
        }
        
        public static class Failed
        {
            public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
            public static string Key(Supplier supplier, int productId)
                => $"offer:refresh:failed:{supplier.ToString()}:{productId}";
        }
        
        public static class Lock
        {
            public static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);
            public static string Key(Supplier supplier, int productId)
                => $"offer:refresh:lock:{supplier.ToString()}:{productId}";
        }
    }
}