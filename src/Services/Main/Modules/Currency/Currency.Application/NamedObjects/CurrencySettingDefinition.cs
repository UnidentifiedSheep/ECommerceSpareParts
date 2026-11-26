using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Enums;
using FluentValidation;
using Locan.Core.Interfaces;
using Main.Entities.Currency;
using Main.Entities.Currency.Settings;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Currency.Application.NamedObjects;

public class CurrencySettingDefinition(
	ISettingsService settingsService,
	IValidator<CurrencySettingInputData> inputValidator)
	: SettingDefinitionBase<
		CurrencySetting,
		CurrencySettingData,
		CurrencySettingInputData,
		CurrencySettingData>(
		settingsService,
		inputValidator)
{
	public override string SystemName => CurrencySetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage => CurrencySettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> CurrencySettingDescriptionMessage.Instance;

	protected override CurrencySettingData Map(CurrencySettingInputData input, CurrencySettingData current)
		=> current with
		{
			RateProvider = input.RateProvider
		};

	protected override CurrencySettingData ToOutput(CurrencySettingData data) => data;

	protected override CurrencySetting CreateSetting(CurrencySettingData data) => new(data);
}

public record CurrencySettingInputData
{
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(SchemaEntity.ExchangeRateProvider)]
	[SchemaFieldLabel(CurrencySettingRateProviderNameMessage.Key)]
	[SchemaFieldDescription(CurrencySettingRateProviderDescriptionMessage.Key)]
	[JsonPropertyName("rateProvider")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required ExchangeRateProvider RateProvider { get; init; }
}
