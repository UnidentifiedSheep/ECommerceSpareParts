using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

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

	public static PricingSetting Default => new(new PricingSettingData());
}

public record PricingSettingData
{
	[JsonPropertyName("version")]
	public Guid Version { get; init; } = Guid.Empty;

	[JsonPropertyName("selectedMarkupId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(nameof(MarkupGroup))]
	[SchemaFieldLabel(PricingSettingSelectedMarkupIdNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingSelectedMarkupIdDescriptionMessage.Key)]
	public int? SelectedMarkupId { get; init; }

	[JsonPropertyName("defaultMarkup")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingDefaultMarkupNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingDefaultMarkupDescriptionMessage.Key)]
	public decimal DefaultMarkup { get; init; } = 0.2m;

	[JsonPropertyName("offerTtl")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingOfferTtlNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingOfferTtlDescriptionMessage.Key)]
	public TimeSpan OfferTtl { get; init; } = TimeSpan.FromDays(1);

	[JsonPropertyName("priceRoundingStep")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingPriceRoundingStepNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingPriceRoundingStepDescriptionMessage.Key)]
	public decimal PriceRoundingStep { get; init; } = 0.01m;

	[JsonPropertyName("deliveryDayPenalty")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingDeliveryDayPenaltyNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingDeliveryDayPenaltyDescriptionMessage.Key)]
	public decimal DeliveryDayPenalty { get; init; } = 2m;

	[JsonPropertyName("uniqProductAdditionalMarkup")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingUniqProductAdditionalMarkupNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingUniqProductAdditionalMarkupDescriptionMessage.Key)]
	public decimal UniqProductAdditionalMarkup { get; init; } = 0.2m;
}
