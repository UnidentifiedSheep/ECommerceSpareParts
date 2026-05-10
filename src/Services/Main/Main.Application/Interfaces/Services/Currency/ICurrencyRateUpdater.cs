using Main.Application.Models.Currency;
using Main.Entities.Setting;

namespace Main.Application.Interfaces.Services.Currency;

public interface ICurrencyRateUpdater
{
    Task<UpdateRatesResult> UpdateAsync(CurrencySetting setting, CancellationToken ct);
}