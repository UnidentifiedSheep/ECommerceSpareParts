using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;
using Pricing.Enums;

namespace Pricing.Entities.Settings;

public class PricingSetting : Setting<PricingSettingData>, ISetting<PricingSetting>
{
    public PricingSetting(string json) : base(SettingName, json) { }

    public PricingSetting(PricingSettingData data) : base(SettingName, data) { }

    public static string SettingName => "PricingSettings";
    public static PricingSetting Default => new(new PricingSettingData());
}

public record PricingSettingData
{
    [JsonPropertyName("pricingStrategy")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductPricingType PricingStrategy { get; init; } = ProductPricingType.Average;

    [JsonPropertyName("selectedMarkupId")]
    public int? SelectedMarkupId { get; init; }

    [JsonPropertyName("defaultMarkup")]
    public decimal DefaultMarkup { get; init; } = 0.2m;
    
    [JsonPropertyName("offerTtl")]
    public TimeSpan? OfferTtl { get; init; }
}