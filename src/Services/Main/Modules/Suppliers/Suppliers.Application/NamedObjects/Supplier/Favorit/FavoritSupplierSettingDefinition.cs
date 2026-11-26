using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using FluentValidation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Suppliers.Settings.Supplier;
using Security.Core.Interfaces;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Favorit;

public class FavoritSupplierSettingDefinition(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor,
	IValidator<FavoritSupplierSettingInputData> inputValidator,
	IValidator<FavoritSupplierSettingData> dataValidator)
	: SettingDefinitionBase<
		FavoritSupplierSetting,
		FavoritSupplierSettingData,
		FavoritSupplierSettingInputData,
		FavoritSupplierSettingOutputData>(
		settingsService,
		inputValidator,
		dataValidator)
{
	public override string SystemName => FavoritSupplierSetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage => SupplierFavoritSettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> SupplierFavoritSettingDescriptionMessage.Instance;

	protected override FavoritSupplierSettingData Map(
		FavoritSupplierSettingInputData input,
		FavoritSupplierSettingData current)
		=> new()
		{
			IsEnabled = input.IsEnabled,
			BaseUrl = SettingValues.Patch(input.BaseUrl, current.BaseUrl, SettingValues.Url),
			EncryptedApiKey = SettingValues.Patch(input.ApiKey, current.EncryptedApiKey, secretEncryptor.Encrypt)
		};

	protected override FavoritSupplierSettingOutputData ToOutput(FavoritSupplierSettingData data)
		=> new()
		{
			IsEnabled = data.IsEnabled,
			BaseUrl = data.BaseUrl,
			HasApiKey = !string.IsNullOrWhiteSpace(data.EncryptedApiKey)
		};

	protected override FavoritSupplierSetting CreateSetting(FavoritSupplierSettingData data) => new(data);
}
