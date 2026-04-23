using Abstractions.Models;
using Main.Abstractions.Models.Settings;

namespace Main.Abstractions.Constants;

public sealed class Settings
{
    public static readonly TypedSetting<CurrencySetting> Currency = new("currency", new CurrencySetting());

    public static readonly TypedSetting[] AllSettings =
    [
        Currency
    ];
}