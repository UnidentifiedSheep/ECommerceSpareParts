namespace Pricing.Application.Static;

public static class CacheKeys
{
    public static class CurrencyCache
    {
        public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

        public static string CurrencyRate(int currencyId)
        {
            return $"currency:{currencyId}:rate";
        }
    }
}
