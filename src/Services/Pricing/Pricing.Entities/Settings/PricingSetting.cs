using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;
using Pricing.Entities;
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
    [RequiredJsonField]
    [InputControl(InputControlType.EnumSelector)]
    [DependsOnEntity(nameof(ProductPricingType))]
    [LocalizedJsonFieldName("pricing.setting.pricing.strategy.name")]
    [LocalizedJsonFieldDescription("pricing.setting.pricing.strategy.description")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductPricingType PricingStrategy { get; init; } = ProductPricingType.Average;

    [JsonPropertyName("selectedMarkupId")]
    [InputControl(InputControlType.EntitySelector)]
    [DependsOnEntity(nameof(MarkupGroup))]
    [LocalizedJsonFieldName("pricing.setting.selected.markup.id.name")]
    [LocalizedJsonFieldDescription("pricing.setting.selected.markup.id.description")]
    public int? SelectedMarkupId { get; init; }

    [JsonPropertyName("defaultMarkup")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.default.markup.name")]
    [LocalizedJsonFieldDescription("pricing.setting.default.markup.description")]
    public decimal DefaultMarkup { get; init; } = 0.2m;
    
    [JsonPropertyName("offerTtl")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.offer.ttl.name")]
    [LocalizedJsonFieldDescription("pricing.setting.offer.ttl.description")]
    public TimeSpan OfferTtl { get; init; } = TimeSpan.FromDays(1);
    
    [JsonPropertyName("priceRoundingStep")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.price.rounding.step.name")]
    [LocalizedJsonFieldDescription("pricing.setting.price.rounding.step.description")]
    public decimal PriceRoundingStep { get; init; } = 0.01m;
    
    [JsonPropertyName("deliveryDayPenalty")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.delivery.day.penalty.name")]
    [LocalizedJsonFieldDescription("pricing.setting.delivery.day.penalty.description")]
    public decimal DeliveryDayPenalty { get; init; } = 2m;
}
