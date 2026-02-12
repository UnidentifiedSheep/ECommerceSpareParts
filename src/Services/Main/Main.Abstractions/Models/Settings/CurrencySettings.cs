using Enums;

namespace Main.Abstractions.Models.Settings;

public record CurrencySettings(int DefaultCurrencyId = 1, bool AutoUpdateRates = true, 
    ExchangeRateProvider RateProvider = ExchangeRateProvider.Cbr);