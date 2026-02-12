using Abstractions.Models;
using Main.Abstractions.Models.Settings;

namespace Main.Abstractions.Constants;

public sealed class Settings
{
    public static readonly TypedSetting<CurrencySettings> Currency = new("currency", new CurrencySettings());
    
    public static readonly TypedSetting[] AllSettings =
    [
        Currency
    ];
}
