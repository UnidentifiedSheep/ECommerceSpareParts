using System.Text.Json;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Exceptions;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Settings;

namespace Main.Application.NamedObjects.SettingDefinitions;

public class GlobalApplicationSettingDefinition(ISettingsService settingsService)
	: SettingDefinitionNamedObjectBase<GlobalApplicationSetting>(settingsService)
{
	public override string SystemName => GlobalApplicationSetting.SettingName;
	public override ILocalizableMessage NameLocalizationMessage
		=> GlobalApplicationSettingNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage
		=> GlobalApplicationSettingDescriptionMessage.Instance;

	public override Type InputSettingType => typeof(GlobalApplicationSettingData);

	public override Type OutputSettingType => typeof(GlobalApplicationSettingData);

	public override async Task UpdateSettingAsync(string json, CancellationToken cancellationToken)
	{
		var input = JsonSerializer.Deserialize<GlobalApplicationSettingData>(json) ??
			throw new InvalidInputException(GlobalApplicationSettingInputInvalidMessage.Instance);

		var data = new GlobalApplicationSettingData
		{
			ApiServiceUrl = NormalizeUrl(input.ApiServiceUrl),
			AppServiceUrl = NormalizeUrl(input.AppServiceUrl)
		};

		await SettingsService.SetSetting(new GlobalApplicationSetting(data), cancellationToken);
	}

	public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken) =>
		(await SettingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken)).Json;

	private static string NormalizeUrl(string? value)
	{
		if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(
				value.Trim(),
				UriKind.Absolute,
				out var uri) || uri.Scheme is not ("http" or "https"))
			throw new InvalidInputException(GlobalApplicationSettingInputInvalidMessage.Instance);

		return uri.AbsoluteUri.TrimEnd('/');
	}
}
