using System.Text.Json;
using System.Text.Json.Serialization;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Exceptions;
using Main.Entities.Settings.Supplier;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.NamedObjects.SettingDefinitions.Supplier;

public class FavoritSupplierSettingDefinition(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor)
	: SettingDefinitionNamedObjectBase<FavoritSupplierSetting>(settingsService)
{
	private const string InvalidInputKey = "supplier.favorit.setting.input.invalid";

	public override string SystemName => FavoritSupplierSetting.SettingName;

	public override string NameLocalizationKey => "supplier.favorit.setting.name";

	public override string DescriptionLocalizationKey => "supplier.favorit.setting.description";

	public override Type InputSettingType => typeof(FavoritSupplierSettingInputData);

	public override Type OutputSettingType => typeof(FavoritSupplierSettingOutputData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var deser = JsonSerializer.Deserialize<FavoritSupplierSettingInputData>(json) ??
			throw new InvalidInputException(InvalidInputKey);

		var currentSetting = await SettingsService.GetOrDefault<FavoritSupplierSetting>(cancellationToken);

		if (!deser.IsEnabled)
		{
			await SettingsService.SetSetting(
				new FavoritSupplierSetting(
					currentSetting.Data with
					{
						IsEnabled = false
					}),
				cancellationToken);
			return;
		}

		var baseUrl = GetBaseUrl(deser.BaseUrl, currentSetting.Data.BaseUrl);
		var encryptedApiKey = GetEncryptedApiKey(deser.ApiKey, currentSetting.Data.EncryptedApiKey);

		if (baseUrl == null || encryptedApiKey == null)
			throw new InvalidInputException(InvalidInputKey);

		var data = new FavoritSupplierSettingData
		{
			IsEnabled = true,
			EncryptedApiKey = encryptedApiKey,
			BaseUrl = baseUrl
		};

		await SettingsService.SetSetting(new FavoritSupplierSetting(data), cancellationToken);
	}

	public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken)
	{
		var setting = await SettingsService.GetOrDefault<FavoritSupplierSetting>(cancellationToken);

		return JsonSerializer.Serialize(
			new FavoritSupplierSettingOutputData
			{
				IsEnabled = setting.Data.IsEnabled,
				BaseUrl = setting.Data.BaseUrl,
				HasApiKey = !string.IsNullOrWhiteSpace(setting.Data.EncryptedApiKey)
			});
	}

	private string? GetBaseUrl(string? inputBaseUrl, string? currentBaseUrl)
	{
		if (inputBaseUrl == null)
			return currentBaseUrl;

		if (!Uri.TryCreate(
				inputBaseUrl.Trim(),
				UriKind.Absolute,
				out var uri) || uri.Scheme is not ("http" or "https"))
			throw new InvalidInputException(InvalidInputKey);

		return uri.AbsoluteUri;
	}

	private string? GetEncryptedApiKey(string? inputApiKey, string? currentEncryptedApiKey)
	{
		if (inputApiKey == null)
			return currentEncryptedApiKey;

		return string.IsNullOrWhiteSpace(inputApiKey)
			? throw new InvalidInputException(InvalidInputKey)
			: secretEncryptor.Encrypt(inputApiKey);
	}
}

public record FavoritSupplierSettingInputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel("supplier.favorit.setting.is.enabled.name")]
	[SchemaFieldDescription("supplier.favorit.setting.is.enabled.description")]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("supplier.favorit.setting.base.url.name")]
	[SchemaFieldDescription("supplier.favorit.setting.base.url.description")]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("apiKey")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("supplier.favorit.setting.api.key.name")]
	[SchemaFieldDescription("supplier.favorit.setting.api.key.description")]
	public string? ApiKey { get; init; }
}

public record FavoritSupplierSettingOutputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel("supplier.favorit.setting.is.enabled.name")]
	[SchemaFieldDescription("supplier.favorit.setting.is.enabled.description")]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaFieldLabel("supplier.favorit.setting.base.url.name")]
	[SchemaFieldDescription("supplier.favorit.setting.base.url.description")]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("hasApiKey")]
	[RequiredSchemaField]
	[SchemaFieldLabel("supplier.favorit.setting.has.api.key.name")]
	[SchemaFieldDescription("supplier.favorit.setting.has.api.key.description")]
	public bool HasApiKey { get; init; }
}
