using Main.Abstractions.Models.Settings;
using Main.Application.Models.Currency;

namespace Main.Application.Interfaces.Services.Currency;

public interface ICurrencyRateUpdater
{
    Task<UpdateRatesResult> UpdateAsync(CurrencySetting setting, CancellationToken ct);
}