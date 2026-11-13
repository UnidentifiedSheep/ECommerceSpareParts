namespace Main.Application.Static;

public static class CurrencyCacheKeys
{
	public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

	public static string Currency(int currencyId) => $"currency:{currencyId}";

	public static string AllCurrencies() => "currencies";
}
