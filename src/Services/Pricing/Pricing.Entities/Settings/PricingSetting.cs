using Domain.CommonEntities;
using Domain.Interfaces;
using Pricing.Enums;

namespace Pricing.Entities.Settings;

public class PricingSetting : Setting<PricingSettingData>, ISetting<PricingSetting>
{
    public PricingSetting(string json) : base(SettingName, json)
    {
    }

    public PricingSetting(PricingSettingData data) : base(SettingName, data)
    {
    }

    public static string SettingName => "PricingSettings";
    public static PricingSetting Default { get; } = new(new PricingSettingData());
}

public record PricingSettingData
{
    public ProductPricingType PricingStrategy { get; init; } = ProductPricingType.Average;
    public int? SelectedMarkupId { get; init; }
    public decimal DefaultMarkup { get; init; } = 20;
}