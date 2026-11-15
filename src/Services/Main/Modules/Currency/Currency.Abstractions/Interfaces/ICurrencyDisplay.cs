namespace Main.Modules.Currency.Abstractions;

public interface ICurrencyDisplay
{
	Task<CurrencyDisplayInfo?> GetAsync(int currencyId, CancellationToken cancellationToken = default);
}

public sealed record CurrencyDisplayInfo(string Code, string Sign);
