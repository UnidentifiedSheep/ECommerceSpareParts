using System.Text.Json;
using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using Exceptions;
using Main.Entities.Settings;

namespace Main.Application.NamedObjects.SettingDefinitions;

public class GlobalApplicationSettingDefinition(
    ISettingsService settingsService
) : SettingDefinitionNamedObjectBase<GlobalApplicationSetting>(settingsService)
{
    private const string InvalidInputKey = "global.application.setting.input.invalid";

    public override string SystemName => GlobalApplicationSetting.SettingName;
    public override string NameLocalizationKey => "global.application.setting.name";
    public override string DescriptionLocalizationKey => "global.application.setting.description";
    public override Type InputSettingType => typeof(GlobalApplicationSettingData);
    public override Type OutputSettingType => typeof(GlobalApplicationSettingData);

    public override async Task UpdateSettingAsync(
        string json,
        CancellationToken cancellationToken)
    {
        var input = JsonSerializer.Deserialize<GlobalApplicationSettingData>(json)
                    ?? throw new InvalidInputException(InvalidInputKey);

        var data = new GlobalApplicationSettingData
        {
            S3ServiceUrl = NormalizeUrl(input.S3ServiceUrl),
            ApiServiceUrl = NormalizeUrl(input.ApiServiceUrl),
            AppServiceUrl = NormalizeUrl(input.AppServiceUrl)
        };

        await SettingsService.SetSetting(
            new GlobalApplicationSetting(data),
            cancellationToken);
    }

    public override async Task<string> GetOutputJsonAsync(CancellationToken cancellationToken)
    {
        return (await SettingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken)).Json;
    }

    private static string NormalizeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https"))
            throw new InvalidInputException(InvalidInputKey);

        return uri.AbsoluteUri.TrimEnd('/');
    }
}
