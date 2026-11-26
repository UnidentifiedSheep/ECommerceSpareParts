using Application.Common.Interfaces.Settings;
using Application.Common.NamedObject;
using FluentValidation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Suppliers.Settings.Supplier;
using Security.Core.Interfaces;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Armtek;

public class ArmtekSupplierSettingDefinition(
	ISettingsService settingsService,
	ISecretEncryptor secretEncryptor,
	IValidator<ArmtekSupplierSettingInputData> inputValidator,
	IValidator<ArmtekSupplierSettingData> dataValidator)
	: SettingDefinitionBase<
		ArmtekSupplierSetting,
		ArmtekSupplierSettingData,
		ArmtekSupplierSettingInputData,
		ArmtekSupplierSettingOutputData>(
		settingsService,
		inputValidator,
		dataValidator)
{
	public override string SystemName => ArmtekSupplierSetting.SettingName;

	public override ILocalizableMessage NameLocalizationMessage => SupplierArmtekSettingNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> SupplierArmtekSettingDescriptionMessage.Instance;

	protected override ArmtekSupplierSettingData Map(
		ArmtekSupplierSettingInputData input,
		ArmtekSupplierSettingData current)
		=> new()
		{
			IsEnabled = input.IsEnabled,
			BaseUrl = SettingValues.Patch(input.BaseUrl, current.BaseUrl, SettingValues.Url),
			Login = SettingValues.Patch(input.Login, current.Login, SettingValues.Text),
			EncryptedPassword = SettingValues.Patch(input.Password, current.EncryptedPassword, secretEncryptor.Encrypt),
			SalesOrganization = SettingValues.Patch(
				input.SalesOrganization,
				current.SalesOrganization,
				SettingValues.Text),
			CustomerCode = SettingValues.Patch(input.CustomerCode, current.CustomerCode, SettingValues.Text),
			ContractCode = SettingValues.Patch(input.ContractCode, current.ContractCode, SettingValues.TextOrNull),
			Storages = SettingValues.Patch(input.Storages, current.Storages, MapStorages)
		};

	protected override ArmtekSupplierSettingOutputData ToOutput(ArmtekSupplierSettingData data)
		=> new()
		{
			IsEnabled = data.IsEnabled,
			BaseUrl = data.BaseUrl,
			Login = data.Login,
			HasPassword = !string.IsNullOrWhiteSpace(data.EncryptedPassword),
			SalesOrganization = data.SalesOrganization,
			CustomerCode = data.CustomerCode,
			ContractCode = data.ContractCode,
			Storages = data
				.Storages
				.Select(x => new ArmtekStorageBindingData
				{
					StorageCode = x.StorageCode, DeliveryMode = x.DeliveryMode, PointCode = x.PointCode
				})
				.ToList()
		};

	protected override ArmtekSupplierSetting CreateSetting(ArmtekSupplierSettingData data) => new(data);

	private static IReadOnlyList<ArmtekStorageBinding> MapStorages(IReadOnlyList<ArmtekStorageBindingData> input)
		=> input
			.Select(x => new ArmtekStorageBinding
			{
				StorageCode = SettingValues.Text(x.StorageCode!),
				DeliveryMode = x.DeliveryMode,
				PointCode = SettingValues.Text(x.PointCode!)
			})
			.ToList();
}
