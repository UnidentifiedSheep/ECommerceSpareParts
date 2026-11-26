using System.Text.Json.Serialization;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using FluentValidation;
using Locan.Core.Interfaces;
using Main.Entities.Storage;
using Main.Entities.Storage.Settings;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Storage.Application.NamedObjects;

public class StorageContentSettingDefinition(
	ISettingsService settingsService,
	IValidator<StorageContentSettingInputData> inputValidator)
	: SettingDefinitionBase<
		StorageContentSetting,
		StorageContentSettingData,
		StorageContentSettingInputData,
		StorageContentSettingData>(
		settingsService,
		inputValidator)
{
	public override string SystemName => StorageContentSetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage => StorageContentSettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> StorageContentSettingDescriptionMessage.Instance;

	protected override StorageContentSettingData Map(
		StorageContentSettingInputData input,
		StorageContentSettingData current)
		=> new()
		{
			StorageContentExtractionPolicy = input.StorageContentExtractionPolicy
		};

	protected override StorageContentSettingData ToOutput(StorageContentSettingData data) => data;

	protected override StorageContentSetting CreateSetting(StorageContentSettingData data) => new(data);
}

public record StorageContentSettingInputData
{
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.NamedObjectSelector)]
	[SchemaDependsOnEntity(SchemaEntity.StorageContentExtractPolicy)]
	[SchemaFieldLabel(StorageContentSettingExtractionPolicyNameMessage.Key)]
	[SchemaFieldDescription(StorageContentSettingExtractionPolicyDescriptionMessage.Key)]
	[JsonPropertyName("storageContentExtractionPolicy")]
	public required string StorageContentExtractionPolicy { get; init; }
}
