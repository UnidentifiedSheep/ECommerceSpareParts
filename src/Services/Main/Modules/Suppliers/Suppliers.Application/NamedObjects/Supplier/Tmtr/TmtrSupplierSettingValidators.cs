using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Suppliers.Settings.Supplier;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Tmtr;

public class TmtrSupplierSettingInputValidator : AbstractValidator<TmtrSupplierSettingInputData>
{
	public TmtrSupplierSettingInputValidator()
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		RuleFor(x => x.BaseUrl).HttpUrl();

		RuleFor(x => x.Login).NotEmptyWhenProvided();

		RuleFor(x => x.Password).NotEmptyWhenProvided();

		RuleFor(x => x.GuaranteedDeliveryOffsetDays).NotNegative();
	}
}

public class TmtrSupplierSettingDataValidator : AbstractValidator<TmtrSupplierSettingData>
{
	public TmtrSupplierSettingDataValidator()
		=> When(
			x => x.IsEnabled,
			() =>
			{
				RuleFor(x => x.BaseUrl).Required();

				RuleFor(x => x.AuthData!.Login)
					.Required()
					.OverridePropertyName(nameof(TmtrSupplierSettingInputData.Login));

				RuleFor(x => x.AuthData!.EncryptedPassword)
					.Required()
					.OverridePropertyName(nameof(TmtrSupplierSettingInputData.Password));
			});
}
