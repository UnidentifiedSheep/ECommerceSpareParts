using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Exceptions;
using Locan.Core.Interfaces;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Entities;
using Main.Entities.Settings;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.NamedObjects.SettingDefinitions;

public class StorageContentSettingDefinition(
	ISettingsService settingsService,
	INamedObjectRegistry<StorageContentExtractPolicyBase> registry)
	: SettingDefinitionNamedObjectBase<StorageContentSetting>(settingsService)
{
	public override string SystemName => StorageContentSetting.SettingName;
	public override ILocalizableMessage NameLocalizationMessage
		=> StorageContentSettingNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage
		=> StorageContentSettingDescriptionMessage.Instance;

	public override Type InputSettingType => typeof(StorageContentSettingInputData);

	public override Type OutputSettingType => typeof(StorageContentSettingData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var deser = JsonSerializer.Deserialize<StorageContentSettingInputData>(json) ??
			throw new InvalidInputException(StorageContentSettingInputInvalidMessage.Instance);

		if (registry.TryGetBySystemName(deser.StorageContentExtractionPolicy) == null)
			throw new InvalidInputException(StorageContentSettingInputInvalidMessage.Instance);

		await SettingsService.SetSetting(
			new StorageContentSetting(
				new StorageContentSettingData
				{
					StorageContentExtractionPolicy = deser.StorageContentExtractionPolicy
				}),
			cancellationToken);
	}

	public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken) =>
		(await SettingsService.GetOrDefault<StorageContentSetting>(cancellationToken)).Json;
}

public record StorageContentSettingInputData
{
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.NamedObjectSelector)]
	[SchemaDependsOnEntity("StorageContentExtractPolicy")]
	[SchemaFieldLabel(StorageContentSettingExtractionPolicyNameMessage.Key)]
	[SchemaFieldDescription(StorageContentSettingExtractionPolicyDescriptionMessage.Key)]
	[JsonPropertyName("storageContentExtractionPolicy")]
	public required string StorageContentExtractionPolicy { get; init; }
}
