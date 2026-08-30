using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Exceptions;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Entities.Settings;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.NamedObjects.SettingDefinitions;

public class StorageContentSettingDefinition(
	ISettingsService settingsService,
	INamedObjectRegistry<StorageContentExtractPolicyBase> registry)
	: SettingDefinitionNamedObjectBase<StorageContentSetting>(settingsService)
{
	private const string InvalidInputKey = "storage.content.setting.input.invalid";

	public override string SystemName => StorageContentSetting.SettingName;

	public override string NameLocalizationKey => "storage.content.setting.name";

	public override string DescriptionLocalizationKey => "storage.content.setting.description";

	public override Type InputSettingType => typeof(StorageContentSettingInputData);

	public override Type OutputSettingType => typeof(StorageContentSettingData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var deser = JsonSerializer.Deserialize<StorageContentSettingInputData>(json) ??
			throw new InvalidInputException(InvalidInputKey);

		if (registry.TryGetBySystemName(deser.StorageContentExtractionPolicy) == null)
			throw new InvalidInputException(InvalidInputKey);

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
	[SchemaFieldLabel("storage.content.setting.extraction.policy.name")]
	[SchemaFieldDescription("storage.content.setting.extraction.policy.description")]
	[JsonPropertyName("storageContentExtractionPolicy")]
	public required string StorageContentExtractionPolicy { get; init; }
}
