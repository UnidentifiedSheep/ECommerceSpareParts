using Main.Application.Models.Currency;
using Main.Entities.Settings;

namespace Main.Application.Interfaces.Services.Currency;

public interface ICurrencyRateUpdater
{
    Task<UpdateRatesResult> UpdateAsync(CurrencySetting setting, CancellationToken ct);
}