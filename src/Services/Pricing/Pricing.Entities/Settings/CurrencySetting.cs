using Domain.CommonEntities;
using Domain.Interfaces;

namespace Pricing.Entities.Settings;

public class CurrencySetting : Setting<CurrencySettingData>, ISetting<CurrencySetting>
{
    public CurrencySetting(string json) : base(SettingName, json)
    {
    }

    public CurrencySetting(CurrencySettingData data) : base(SettingName, data)
    {
    }

    public static string SettingName => "CurrencySetting";
    public static CurrencySetting Default { get; } = new(new CurrencySettingData());
}

public record CurrencySettingData
{
    public int BaseCurrencyId { get; init; } = 1;
}