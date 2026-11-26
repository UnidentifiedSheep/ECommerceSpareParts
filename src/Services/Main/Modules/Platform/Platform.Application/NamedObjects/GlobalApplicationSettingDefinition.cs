using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using FluentValidation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Platform.Settings;

namespace Main.Modules.Platform.Application.NamedObjects;

public class GlobalApplicationSettingDefinition(
	ISettingsService settingsService,
	IValidator<GlobalApplicationSettingData> inputValidator)
	: SettingDefinitionBase<
		GlobalApplicationSetting,
		GlobalApplicationSettingData,
		GlobalApplicationSettingData,
		GlobalApplicationSettingData>(
		settingsService,
		inputValidator)
{
	public override string SystemName => GlobalApplicationSetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage
		=> GlobalApplicationSettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> GlobalApplicationSettingDescriptionMessage.Instance;

	protected override GlobalApplicationSettingData Map(
		GlobalApplicationSettingData input,
		GlobalApplicationSettingData current)
		=> new()
		{
			ApiServiceUrl = SettingValues.Url(input.ApiServiceUrl!),
			AppServiceUrl = SettingValues.Url(input.AppServiceUrl!)
		};

	protected override GlobalApplicationSettingData ToOutput(GlobalApplicationSettingData data) => data;

	protected override GlobalApplicationSetting CreateSetting(GlobalApplicationSettingData data) => new(data);
}
