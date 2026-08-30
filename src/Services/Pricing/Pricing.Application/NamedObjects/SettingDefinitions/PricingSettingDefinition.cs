using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Contracts.Analytics;
using Exceptions;
using MassTransit;
using Pricing.Entities;
using Pricing.Entities.Settings;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Pricing.Application.NamedObjects.SettingDefinitions;

public class PricingSettingDefinition(ISettingsService settingsService, IPublishEndpoint publishEndpoint)
	: SettingDefinitionNamedObjectBase<PricingSetting>(settingsService)
{
	private const string InvalidInputKey = "pricing.setting.input.invalid";

	public override string SystemName => PricingSetting.SettingName;

	public override string NameLocalizationKey => "pricing.setting.name";

	public override string DescriptionLocalizationKey => "pricing.setting.description";

	public override Type InputSettingType => typeof(PricingSettingInputData);

	public override Type OutputSettingType => typeof(PricingSettingData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var deser = JsonSerializer.Deserialize<PricingSettingInputData>(json) ??
			throw new InvalidInputException(InvalidInputKey);

		Validate(deser);

		await SettingsService.SetSetting(
			new PricingSetting(
				new PricingSettingData
				{
					Version = Guid.NewGuid(),
					SelectedMarkupId = deser.SelectedMarkupId,
					DefaultMarkup = deser.DefaultMarkup,
					OfferTtl = deser.OfferTtl,
					PriceRoundingStep = deser.PriceRoundingStep,
					DeliveryDayPenalty = deser.DeliveryDayPenalty,
					UniqProductAdditionalMarkup = deser.UniqProductAdditionalMarkup
				}),
			cancellationToken);

		await publishEndpoint.Publish(new MarkupRangesRefreshRequestedEvent(), cancellationToken);
	}

	public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken) =>
		(await SettingsService.GetOrDefault<PricingSetting>(cancellationToken)).Json;

	private static void Validate(PricingSettingInputData input)
	{
		if (input.SelectedMarkupId <= 0 || input.DefaultMarkup < 0 || input.OfferTtl <= TimeSpan.Zero ||
			input.PriceRoundingStep <= 0 || input.DeliveryDayPenalty < 0 ||
			input.UniqProductAdditionalMarkup <= 0)
			throw new InvalidInputException(InvalidInputKey);
	}
}

public record PricingSettingInputData
{
	[JsonPropertyName("selectedMarkupId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(typeof(MarkupGroup), "id")]
	[SchemaFieldLabel("pricing.setting.selected.markup.id.name")]
	[SchemaFieldDescription("pricing.setting.selected.markup.id.description")]
	public int? SelectedMarkupId { get; init; }

	[JsonPropertyName("defaultMarkup")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("pricing.setting.default.markup.name")]
	[SchemaFieldDescription("pricing.setting.default.markup.description")]
	public required decimal DefaultMarkup { get; init; }

	[JsonPropertyName("offerTtl")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("pricing.setting.offer.ttl.name")]
	[SchemaFieldDescription("pricing.setting.offer.ttl.description")]
	public required TimeSpan OfferTtl { get; init; }

	[JsonPropertyName("priceRoundingStep")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("pricing.setting.price.rounding.step.name")]
	[SchemaFieldDescription("pricing.setting.price.rounding.step.description")]
	public required decimal PriceRoundingStep { get; init; }

	[JsonPropertyName("deliveryDayPenalty")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("pricing.setting.delivery.day.penalty.name")]
	[SchemaFieldDescription("pricing.setting.delivery.day.penalty.description")]
	public required decimal DeliveryDayPenalty { get; init; }

	[JsonPropertyName("uniqProductAdditionalMarkup")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("pricing.setting.uniq.product.additional.markup.name")]
	[SchemaFieldDescription("pricing.setting.uniq.product.additional.markup.description")]
	public required decimal UniqProductAdditionalMarkup { get; init; }
}
