using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Suppliers.Settings.Supplier;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Armtek;

public class ArmtekSupplierSettingInputValidator : AbstractValidator<ArmtekSupplierSettingInputData>
{
	public ArmtekSupplierSettingInputValidator()
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		RuleFor(x => x.BaseUrl).HttpUrl();

		RuleFor(x => x.Login).NotEmptyWhenProvided();

		RuleFor(x => x.Password).NotEmptyWhenProvided();

		RuleFor(x => x.SalesOrganization)
			.NotEmptyWhenProvided()
			.MaximumTrimmedLength(ArmtekSupplierSettingData.SalesOrganizationMaxLength);

		RuleFor(x => x.CustomerCode)
			.NotEmptyWhenProvided()
			.MaximumTrimmedLength(ArmtekSupplierSettingData.CustomerCodeMaxLength);

		RuleFor(x => x.ContractCode).MaximumTrimmedLength(ArmtekSupplierSettingData.ContractCodeMaxLength);

		RuleFor(x => x.Storages).UniqueBy(x => x.StorageCode?.Trim(), StringComparer.OrdinalIgnoreCase);

		RuleForEach(x => x.Storages)
			.ChildRules(storage =>
			{
				storage.RuleLevelCascadeMode = CascadeMode.Stop;

				storage.RuleFor(x => x.StorageCode).Required();

				storage.RuleFor(x => x.DeliveryMode).DefinedEnum();

				storage.RuleFor(x => x.PointCode)
					.Required()
					.MaximumTrimmedLength(ArmtekStorageBinding.PointCodeMaxLength);
			});
	}
}

public class ArmtekSupplierSettingDataValidator : AbstractValidator<ArmtekSupplierSettingData>
{
	public ArmtekSupplierSettingDataValidator()
		=> When(
			x => x.IsEnabled,
			() =>
			{
				RuleFor(x => x.BaseUrl).Required();

				RuleFor(x => x.Login).Required();

				RuleFor(x => x.EncryptedPassword)
					.Required()
					.OverridePropertyName(nameof(ArmtekSupplierSettingInputData.Password));

				RuleFor(x => x.SalesOrganization).Required();

				RuleFor(x => x.CustomerCode).Required();
			});
}
