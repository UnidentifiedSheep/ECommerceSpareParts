using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Balance.UpdateOrganizationFinancialProfile;

public class
	UpdateOrganizationFinancialProfileValidation : AbstractValidator<
	UpdateOrganizationFinancialProfileCommand>
{
	public UpdateOrganizationFinancialProfileValidation()
	{
		When(
			x => x.Patch.MinimalAllowedBalance.IsSet,
			() =>
			{
				RuleFor(x => x.Patch.MinimalAllowedBalance.Value)
					.LessThanOrEqualTo(0)
					.WithLocalizableError(FinancialProfileMinAllowedBalanceMustNotBePositiveMessage.Instance)
					.PrecisionScale(
						18,
						2,
						true)
					.WithLocalizableError(
						FinancialProfileMinAllowedBalanceMaxTwoDecimalPlacesMessage.Instance);
			});
	}
}
