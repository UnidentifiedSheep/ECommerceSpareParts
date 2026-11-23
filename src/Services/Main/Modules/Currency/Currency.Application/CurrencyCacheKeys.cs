namespace Main.Modules.Currency.Application;

public static class CurrencyCacheKeys
{
	public const string Currencies = "currency:catalog";

	public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);
}
