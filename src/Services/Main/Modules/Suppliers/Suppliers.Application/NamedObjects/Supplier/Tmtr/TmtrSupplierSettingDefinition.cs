using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using FluentValidation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Suppliers.Settings.Supplier;
using Security.Core.Interfaces;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Tmtr;

public class TmtrSupplierSettingDefinition(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor,
	IValidator<TmtrSupplierSettingInputData> inputValidator,
	IValidator<TmtrSupplierSettingData> dataValidator)
	: SettingDefinitionBase<
		TmtrSupplierSetting,
		TmtrSupplierSettingData,
		TmtrSupplierSettingInputData,
		TmtrSupplierSettingOutputData>(
		settingsService,
		inputValidator,
		dataValidator)
{
	public override string SystemName => TmtrSupplierSetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage => SupplierTmtrSettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> SupplierTmtrSettingDescriptionMessage.Instance;

	protected override TmtrSupplierSettingData Map(TmtrSupplierSettingInputData input, TmtrSupplierSettingData current)
		=> new()
		{
			IsEnabled = input.IsEnabled,
			BaseUrl = SettingValues.Patch(input.BaseUrl, current.BaseUrl, SettingValues.Url),
			GuaranteedDeliveryOffsetDays = input.GuaranteedDeliveryOffsetDays,
			AuthData = new TmtrAuthData
			{
				Login = SettingValues.Patch(input.Login, current.AuthData?.Login, SettingValues.Text),
				EncryptedPassword = SettingValues.Patch(
					input.Password,
					current.AuthData?.EncryptedPassword,
					secretEncryptor.Encrypt)
			}
		};

	protected override TmtrSupplierSettingOutputData ToOutput(TmtrSupplierSettingData data)
		=> new()
		{
			IsEnabled = data.IsEnabled,
			BaseUrl = data.BaseUrl,
			GuaranteedDeliveryOffsetDays = data.GuaranteedDeliveryOffsetDays,
			Login = data.AuthData?.Login,
			HasPassword = !string.IsNullOrWhiteSpace(data.AuthData?.EncryptedPassword)
		};

	protected override TmtrSupplierSetting CreateSetting(TmtrSupplierSettingData data) => new(data);
}
