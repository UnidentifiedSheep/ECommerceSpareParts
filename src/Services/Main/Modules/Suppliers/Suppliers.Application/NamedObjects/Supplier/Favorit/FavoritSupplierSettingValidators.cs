using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Suppliers.Settings.Supplier;

namespace Main.Modules.Suppliers.Application.NamedObjects.Supplier.Favorit;

public class FavoritSupplierSettingInputValidator : AbstractValidator<FavoritSupplierSettingInputData>
{
	public FavoritSupplierSettingInputValidator()
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		RuleFor(x => x.BaseUrl).HttpUrl();

		RuleFor(x => x.ApiKey).NotEmptyWhenProvided();
	}
}

public class FavoritSupplierSettingDataValidator : AbstractValidator<FavoritSupplierSettingData>
{
	public FavoritSupplierSettingDataValidator()
		=> When(
			x => x.IsEnabled,
			() =>
			{
				RuleFor(x => x.BaseUrl).Required();

				RuleFor(x => x.EncryptedApiKey)
					.Required()
					.OverridePropertyName(nameof(FavoritSupplierSettingInputData.ApiKey));
			});
}
