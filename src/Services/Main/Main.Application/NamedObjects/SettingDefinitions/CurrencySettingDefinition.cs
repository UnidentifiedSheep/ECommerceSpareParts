using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Attributes.JsonAttributes;
using Enums;
using Exceptions;
using Main.Entities.Settings;

namespace Main.Application.NamedObjects.SettingDefinitions;

public class CurrencySettingDefinition(
    ISettingsService settingsService
) : SettingDefinitionNamedObjectBase<CurrencySetting>(settingsService)
{
    public override string SystemName => CurrencySetting.SettingName;
    public override string NameLocalizationKey => "currency.setting.name";
    public override string DescriptionLocalizationKey => "currency.setting.description";
    public override Type InputSettingType => typeof(CurrencySettingInputData);
    public override Type OutputSettingType => typeof(CurrencySettingData);

    public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
    {
        var deser = JsonSerializer.Deserialize<CurrencySettingInputData>(json)
                    ?? throw new InvalidInputException("currency.setting.input.invalid");
        var currentSetting = await SettingsService.GetOrDefault<CurrencySetting>(cancellationToken);
        await SettingsService.SetSetting(
            new CurrencySetting(
                new CurrencySettingData
                {
                    BaseCurrencyId = currentSetting.Data.BaseCurrencyId,
                    RateProvider = deser.RateProvider
                }),
            cancellationToken);
    }

    public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken)
    {
        return (await SettingsService.GetOrDefault<CurrencySetting>(cancellationToken)).Json;
    }
}

public record CurrencySettingInputData
{
    [RequiredJsonField]
    [InputControl(InputControlType.EnumSelector)]
    [DependsOnEntity(nameof(ExchangeRateProvider))]
    [LocalizedJsonFieldName("currency.setting.rate.provider.name")]
    [LocalizedJsonFieldDescription("currency.setting.rate.provider.description")]
    [JsonPropertyName("rateProvider")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ExchangeRateProvider RateProvider { get; init; }
}