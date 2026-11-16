using Main.Entities.Currency.Settings;
using Main.Modules.Currency.Application.Models.Currency;

namespace Main.Modules.Currency.Application.Interfaces;

public interface ICurrencyRateUpdater
{
	Task<UpdateRatesResult> UpdateAsync(CurrencySetting setting, CancellationToken ct);
}
