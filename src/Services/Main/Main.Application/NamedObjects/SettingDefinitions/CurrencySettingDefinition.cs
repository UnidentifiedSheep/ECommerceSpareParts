using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Attributes.JsonAttributes;
using Domain.CommonEntities;
using Enums;
using Exceptions;
using Main.Entities.Setting;

namespace Main.Application.NamedObjects.SettingDefinitions;

public class CurrencySettingDefinition(
    ISettingsService settingsService
    ) : SettingDefinitionNamedObjectBase
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
        var currentSetting = await settingsService.GetOrDefault<CurrencySetting>(cancellationToken);
        await settingsService.SetSetting(
            new CurrencySetting(new CurrencySettingData
            {
                BaseCurrencyId = currentSetting.Data.BaseCurrencyId,
                RateProvider = deser.RateProvider
            }), 
            cancellationToken);
    }

    public override async Task<Setting> GetSettingAsync(CancellationToken cancellationToken)
        => await settingsService.GetOrDefault<CurrencySetting>(cancellationToken);
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
