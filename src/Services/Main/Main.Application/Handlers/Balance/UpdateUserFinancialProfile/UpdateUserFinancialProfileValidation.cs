using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Balance.UpdateUserFinancialProfile;

public class UpdateUserFinancialProfileValidation : AbstractValidator<UpdateUserFinancialProfileCommand>
{
    public UpdateUserFinancialProfileValidation()
    {
        When(
            x => x.Patch.MinimalAllowedBalance.IsSet,
            () =>
            {
                RuleFor(x => x.Patch.MinimalAllowedBalance.Value)
                    .LessThanOrEqualTo(0)
                    .WithLocalizationKey("financial.profile.min.allowed.balance.must.not.be.positive")
                    .PrecisionScale(
                        18,
                        2,
                        true)
                    .WithLocalizationKey("financial.profile.min.allowed.balance.max.two.decimal.places");
            });
    }
}