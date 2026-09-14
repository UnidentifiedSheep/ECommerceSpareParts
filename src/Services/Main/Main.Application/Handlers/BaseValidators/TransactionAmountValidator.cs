using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.BaseValidators;

public class TransactionAmountValidator : AbstractValidator<decimal>
{
	public TransactionAmountValidator()
	{
		RuleFor(x => x).GreaterThan(0).WithLocalizableError(TransactionAmountMustBePositiveMessage.Instance);

		RuleFor(x => x)
			.PrecisionScale(
				18,
				2,
				true)
			.WithLocalizableError(TransactionAmountMaxTwoDecimalPlacesMessage.Instance);
	}
}
