using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using FluentValidation;
using Locan.Core.Interfaces;
using MassTransit;
using Pricing.Application.Events;
using Pricing.Entities;
using Pricing.Entities.Settings;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Pricing.Application.NamedObjects.SettingDefinitions;

public class PricingSettingDefinition(
	ISettingsService settingsService,
	IPublishEndpoint publishEndpoint,
	IValidator<PricingSettingInputData> inputValidator)
	: SettingDefinitionBase<
		PricingSetting,
		PricingSettingData,
		PricingSettingInputData,
		PricingSettingData>(
		settingsService,
		inputValidator)
{
	public override string SystemName => PricingSetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage => PricingSettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> PricingSettingDescriptionMessage.Instance;

	protected override PricingSettingData Map(PricingSettingInputData input, PricingSettingData current)
		=> new()
		{
			Version = Guid.NewGuid(),
			SelectedMarkupId = input.SelectedMarkupId,
			DefaultMarkup = input.DefaultMarkup,
			OfferTtl = input.OfferTtl,
			PriceRoundingStep = input.PriceRoundingStep,
			DeliveryDayPenalty = input.DeliveryDayPenalty,
			UniqProductAdditionalMarkup = input.UniqProductAdditionalMarkup
		};

	protected override PricingSettingData ToOutput(PricingSettingData data) => data;

	protected override PricingSetting CreateSetting(PricingSettingData data) => new(data);

	protected override Task OnUpdatedAsync(PricingSettingData data, CancellationToken cancellationToken)
		=> publishEndpoint.Publish(new MarkupRangesRefreshRequestedEvent(), cancellationToken);
}

public record PricingSettingInputData
{
	[JsonPropertyName("selectedMarkupId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.MarkupGroup, "id")]
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
