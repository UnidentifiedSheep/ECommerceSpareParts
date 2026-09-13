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

public class TmtrSupplierSettingDefinition(ISettingsService settingsService, ISecretEncryptor secretEncryptor)
	: SettingDefinitionNamedObjectBase<TmtrSupplierSetting>(settingsService)
{
	public override string SystemName => TmtrSupplierSetting.SettingName;
	public override ILocalizableMessage NameLocalizationMessage
		=> SupplierTmtrSettingNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage
		=> SupplierTmtrSettingDescriptionMessage.Instance;

	public override Type InputSettingType => typeof(TmtrSupplierSettingInputData);

	public override Type OutputSettingType => typeof(TmtrSupplierSettingOutputData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var input = JsonSerializer.Deserialize<TmtrSupplierSettingInputData>(json) ??
			throw new InvalidInputException(SupplierTmtrSettingInputInvalidMessage.Instance);
		var current = await SettingsService.GetOrDefault<TmtrSupplierSetting>(cancellationToken);

		if (!input.IsEnabled)
		{
			await SettingsService.SetSetting(
				new TmtrSupplierSetting(
					current.Data with
					{
						IsEnabled = false
					}),
				cancellationToken);
			return;
		}

		var baseUrl = GetBaseUrl(input.BaseUrl, current.Data.BaseUrl);
		var login = GetLogin(input.Login, current.Data.AuthData?.Login);
		var encryptedPassword = GetEncryptedPassword(
			input.Password,
			current.Data.AuthData?.EncryptedPassword);

		if (baseUrl is null || login is null || encryptedPassword is null ||
			input.GuaranteedDeliveryOffsetDays < 0)
			throw new InvalidInputException(SupplierTmtrSettingInputInvalidMessage.Instance);

		var data = new TmtrSupplierSettingData
		{
			IsEnabled = true,
			BaseUrl = baseUrl,
			GuaranteedDeliveryOffsetDays = input.GuaranteedDeliveryOffsetDays,
			AuthData = new TmtrAuthData
			{
				Login = login, EncryptedPassword = encryptedPassword
			}
		};

		await SettingsService.SetSetting(new TmtrSupplierSetting(data), cancellationToken);
	}

	public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken)
	{
		var setting = await SettingsService.GetOrDefault<TmtrSupplierSetting>(cancellationToken);

		return JsonSerializer.Serialize(
			new TmtrSupplierSettingOutputData
			{
				IsEnabled = setting.Data.IsEnabled,
				BaseUrl = setting.Data.BaseUrl,
				GuaranteedDeliveryOffsetDays = setting.Data.GuaranteedDeliveryOffsetDays,
				Login = setting.Data.AuthData?.Login,
				HasPassword = !string.IsNullOrWhiteSpace(setting.Data.AuthData?.EncryptedPassword)
			});
	}

	private static string? GetBaseUrl(string? inputBaseUrl, string? currentBaseUrl)
	{
		if (inputBaseUrl is null)
			return currentBaseUrl;

		if (!Uri.TryCreate(
				inputBaseUrl.Trim(),
				UriKind.Absolute,
				out var uri) || uri.Scheme is not ("http" or "https"))
			throw new InvalidInputException(SupplierTmtrSettingInputInvalidMessage.Instance);

		return uri.AbsoluteUri;
	}

	private static string? GetLogin(string? inputLogin, string? currentLogin)
	{
		if (inputLogin is null)
			return currentLogin;

		return string.IsNullOrWhiteSpace(inputLogin)
			? throw new InvalidInputException(SupplierTmtrSettingInputInvalidMessage.Instance)
			: inputLogin.Trim();
	}

	private string? GetEncryptedPassword(string? inputPassword, string? currentEncryptedPassword)
	{
		if (inputPassword is null)
			return currentEncryptedPassword;

		return string.IsNullOrWhiteSpace(inputPassword)
			? throw new InvalidInputException(SupplierTmtrSettingInputInvalidMessage.Instance)
			: secretEncryptor.Encrypt(inputPassword);
	}
}

public record TmtrSupplierSettingInputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierTmtrSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("guaranteedDeliveryOffsetDays")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingDeliveryOffsetNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingDeliveryOffsetDescriptionMessage.Key)]
	public int GuaranteedDeliveryOffsetDays { get; init; } = 1;

	[JsonPropertyName("login")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierTmtrSettingLoginNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingLoginDescriptionMessage.Key)]
	public string? Login { get; init; }

	[JsonPropertyName("password")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(SupplierTmtrSettingPasswordNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingPasswordDescriptionMessage.Key)]
	public string? Password { get; init; }
}

public record TmtrSupplierSettingOutputData
{
	[JsonPropertyName("isEnabled")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingIsEnabledNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingIsEnabledDescriptionMessage.Key)]
	public bool IsEnabled { get; init; }

	[JsonPropertyName("baseUrl")]
	[SchemaFieldLabel(SupplierTmtrSettingBaseUrlNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingBaseUrlDescriptionMessage.Key)]
	public string? BaseUrl { get; init; }

	[JsonPropertyName("guaranteedDeliveryOffsetDays")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingDeliveryOffsetNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingDeliveryOffsetDescriptionMessage.Key)]
	public int GuaranteedDeliveryOffsetDays { get; init; }

	[JsonPropertyName("login")]
	[SchemaFieldLabel(SupplierTmtrSettingLoginNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingLoginDescriptionMessage.Key)]
	public string? Login { get; init; }

	[JsonPropertyName("hasPassword")]
	[RequiredSchemaField]
	[SchemaFieldLabel(SupplierTmtrSettingHasPasswordNameMessage.Key)]
	[SchemaFieldDescription(SupplierTmtrSettingHasPasswordDescriptionMessage.Key)]
	public bool HasPassword { get; init; }
}
