using System.Text.Json;
using System.Text.Json.Serialization;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Exceptions;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Settings.Supplier;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Application.NamedObjects.SettingDefinitions.Supplier;

public class FavoritSupplierSettingDefinition(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor)
	: SettingDefinitionNamedObjectBase<FavoritSupplierSetting>(settingsService)
{
	public override string SystemName => FavoritSupplierSetting.SettingName;
	public override ILocalizableMessage NameLocalizationMessage
		=> SupplierFavoritSettingNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage
		=> SupplierFavoritSettingDescriptionMessage.Instance;

	public override Type InputSettingType => typeof(FavoritSupplierSettingInputData);

	public override Type OutputSettingType => typeof(FavoritSupplierSettingOutputData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var deser = JsonSerializer.Deserialize<FavoritSupplierSettingInputData>(json) ??
			throw new InvalidInputException(SupplierFavoritSettingInputInvalidMessage.Instance);

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
			throw new InvalidInputException(SupplierFavoritSettingInputInvalidMessage.Instance);

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
			throw new InvalidInputException(SupplierFavoritSettingInputInvalidMessage.Instance);

		return uri.AbsoluteUri;
	}

	private string? GetEncryptedApiKey(string? inputApiKey, string? currentEncryptedApiKey)
	{
		if (inputApiKey == null)
			return currentEncryptedApiKey;

		return string.IsNullOrWhiteSpace(inputApiKey)
			? throw new InvalidInputException(SupplierFavoritSettingInputInvalidMessage.Instance)
			: secretEncryptor.Encrypt(inputApiKey);
	}
}

public record FavoritSupplierSettingInputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierFavoritSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierFavoritSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("apiKey")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierFavoritSettingApiKeyNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingApiKeyDescriptionMessage.Key)]
	public string? ApiKey { get; init; }
}

public record FavoritSupplierSettingOutputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierFavoritSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaFieldLabel(SupplierFavoritSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("hasApiKey")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierFavoritSettingHasApiKeyNameMessage.Key)]
	[SchemaFieldDescription(SupplierFavoritSettingHasApiKeyDescriptionMessage.Key)]
	public bool HasApiKey { get; init; }
}
