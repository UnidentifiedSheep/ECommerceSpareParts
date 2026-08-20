using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using Domain.CommonEntities;
using Domain.Interfaces;
using SchemaGeneration.Abstractions.Enums;
using Pricing.Entities;

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
    [JsonPropertyName("version")]
    public Guid Version { get; init; } = Guid.Empty;

    [JsonPropertyName("selectedMarkupId")]
    [SchemaInputControl(InputControlType.EntitySelector)]
    [SchemaDependsOnEntity(nameof(MarkupGroup))]
    [SchemaFieldLabel("pricing.setting.selected.markup.id.name")]
    [SchemaFieldDescription("pricing.setting.selected.markup.id.description")]
    public int? SelectedMarkupId { get; init; }

    [JsonPropertyName("defaultMarkup")]
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.TextField)]
    [SchemaFieldLabel("pricing.setting.default.markup.name")]
    [SchemaFieldDescription("pricing.setting.default.markup.description")]
    public decimal DefaultMarkup { get; init; } = 0.2m;
    
    [JsonPropertyName("offerTtl")]
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.TextField)]
    [SchemaFieldLabel("pricing.setting.offer.ttl.name")]
    [SchemaFieldDescription("pricing.setting.offer.ttl.description")]
    public TimeSpan OfferTtl { get; init; } = TimeSpan.FromDays(1);
    
    [JsonPropertyName("priceRoundingStep")]
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.TextField)]
    [SchemaFieldLabel("pricing.setting.price.rounding.step.name")]
    [SchemaFieldDescription("pricing.setting.price.rounding.step.description")]
    public decimal PriceRoundingStep { get; init; } = 0.01m;
    
    [JsonPropertyName("deliveryDayPenalty")]
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.TextField)]
    [SchemaFieldLabel("pricing.setting.delivery.day.penalty.name")]
    [SchemaFieldDescription("pricing.setting.delivery.day.penalty.description")]
    public decimal DeliveryDayPenalty { get; init; } = 2m;
    
    [JsonPropertyName("uniqProductAdditionalMarkup")]
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.TextField)]
    [SchemaFieldLabel("pricing.setting.uniq.product.additional.markup.name")]
    [SchemaFieldDescription("pricing.setting.uniq.product.additional.markup.description")]
    public decimal UniqProductAdditionalMarkup { get; init; } = 0.2m;
}
