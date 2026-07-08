using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Attributes.JsonAttributes;
using Enums;
using Exceptions;
using Pricing.Entities;
using Pricing.Entities.Settings;
using Pricing.Enums;

namespace Pricing.Application.NamedObjects.SettingDefinitions;

public class PricingSettingDefinition(
    ISettingsService settingsService
) : SettingDefinitionNamedObjectBase<PricingSetting>(settingsService)
{
    private const string InvalidInputKey = "pricing.setting.input.invalid";

    public override string SystemName => PricingSetting.SettingName;
    public override string NameLocalizationKey => "pricing.setting.name";
    public override string DescriptionLocalizationKey => "pricing.setting.description";
    public override Type InputSettingType => typeof(PricingSettingInputData);
    public override Type OutputSettingType => typeof(PricingSettingData);

    public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
    {
        var deser = JsonSerializer.Deserialize<PricingSettingInputData>(json)
                    ?? throw new InvalidInputException(InvalidInputKey);

        Validate(deser);

        await SettingsService.SetSetting(
            new PricingSetting(
                new PricingSettingData
                {
                    PricingStrategy = deser.PricingStrategy,
                    SelectedMarkupId = deser.SelectedMarkupId,
                    DefaultMarkup = deser.DefaultMarkup,
                    OfferTtl = deser.OfferTtl,
                    PriceRoundingStep = deser.PriceRoundingStep,
                    DeliveryDayPenalty = deser.DeliveryDayPenalty,
                    UniqProductAdditionalMarkup = deser.UniqProductAdditionalMarkup
                }),
            cancellationToken);
    }

    public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken)
    {
        return (await SettingsService.GetOrDefault<PricingSetting>(cancellationToken)).Json;
    }

    private static void Validate(PricingSettingInputData input)
    {
        if (input.SelectedMarkupId <= 0 ||
            input.DefaultMarkup < 0 ||
            input.OfferTtl <= TimeSpan.Zero ||
            input.PriceRoundingStep <= 0 ||
            input.DeliveryDayPenalty < 0 ||
            input.UniqProductAdditionalMarkup <= 0)
            throw new InvalidInputException(InvalidInputKey);
    }
}

public record PricingSettingInputData
{
    [JsonPropertyName("pricingStrategy")]
    [RequiredJsonField]
    [InputControl(InputControlType.EnumSelector)]
    [DependsOnEntity(nameof(ProductPricingType))]
    [LocalizedJsonFieldName("pricing.setting.pricing.strategy.name")]
    [LocalizedJsonFieldDescription("pricing.setting.pricing.strategy.description")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ProductPricingType PricingStrategy { get; init; }

    [JsonPropertyName("selectedMarkupId")]
    [InputControl(InputControlType.EntitySelector)]
    [DependsOnEntity(typeof(MarkupGroup), "id")]
    [LocalizedJsonFieldName("pricing.setting.selected.markup.id.name")]
    [LocalizedJsonFieldDescription("pricing.setting.selected.markup.id.description")]
    public int? SelectedMarkupId { get; init; }

    [JsonPropertyName("defaultMarkup")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.default.markup.name")]
    [LocalizedJsonFieldDescription("pricing.setting.default.markup.description")]
    public required decimal DefaultMarkup { get; init; }

    [JsonPropertyName("offerTtl")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.offer.ttl.name")]
    [LocalizedJsonFieldDescription("pricing.setting.offer.ttl.description")]
    public required TimeSpan OfferTtl { get; init; }

    [JsonPropertyName("priceRoundingStep")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.price.rounding.step.name")]
    [LocalizedJsonFieldDescription("pricing.setting.price.rounding.step.description")]
    public required decimal PriceRoundingStep { get; init; }

    [JsonPropertyName("deliveryDayPenalty")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.delivery.day.penalty.name")]
    [LocalizedJsonFieldDescription("pricing.setting.delivery.day.penalty.description")]
    public required decimal DeliveryDayPenalty { get; init; }
    
    [JsonPropertyName("uniqProductAdditionalMarkup")]
    [RequiredJsonField]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("pricing.setting.uniq.product.additional.markup.name")]
    [LocalizedJsonFieldDescription("pricing.setting.uniq.product.additional.markup.description")]
    public required decimal UniqProductAdditionalMarkup { get; init; }
}
