using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Contracts.Analytics;
using Exceptions;
using Locan.Core.Interfaces;
using MassTransit;
using Pricing.Entities;
using Pricing.Entities.Settings;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Pricing.Application.NamedObjects.SettingDefinitions;

public class PricingSettingDefinition(ISettingsService settingsService, IPublishEndpoint publishEndpoint)
	: SettingDefinitionNamedObjectBase<PricingSetting>(settingsService)
{
	public override string SystemName => PricingSetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage => PricingSettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => PricingSettingDescriptionMessage.Instance;

	public override Type InputSettingType => typeof(PricingSettingInputData);

	public override Type OutputSettingType => typeof(PricingSettingData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var deser = JsonSerializer.Deserialize<PricingSettingInputData>(json) ??
			throw new InvalidInputException(PricingSettingInputInvalidMessage.Instance);

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
			throw new InvalidInputException(PricingSettingInputInvalidMessage.Instance);
	}
}

public record PricingSettingInputData
{
	[JsonPropertyName("selectedMarkupId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(typeof(MarkupGroup), "id")]
	[SchemaFieldLabel(PricingSettingSelectedMarkupIdNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingSelectedMarkupIdDescriptionMessage.Key)]
	public int? SelectedMarkupId { get; init; }

	[JsonPropertyName("defaultMarkup")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingDefaultMarkupNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingDefaultMarkupDescriptionMessage.Key)]
	public required decimal DefaultMarkup { get; init; }

	[JsonPropertyName("offerTtl")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingOfferTtlNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingOfferTtlDescriptionMessage.Key)]
	public required TimeSpan OfferTtl { get; init; }

	[JsonPropertyName("priceRoundingStep")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingPriceRoundingStepNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingPriceRoundingStepDescriptionMessage.Key)]
	public required decimal PriceRoundingStep { get; init; }

	[JsonPropertyName("deliveryDayPenalty")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingDeliveryDayPenaltyNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingDeliveryDayPenaltyDescriptionMessage.Key)]
	public required decimal DeliveryDayPenalty { get; init; }

	[JsonPropertyName("uniqProductAdditionalMarkup")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(PricingSettingUniqProductAdditionalMarkupNameMessage.Key)]
	[SchemaFieldDescription(PricingSettingUniqProductAdditionalMarkupDescriptionMessage.Key)]
	public required decimal UniqProductAdditionalMarkup { get; init; }
}
