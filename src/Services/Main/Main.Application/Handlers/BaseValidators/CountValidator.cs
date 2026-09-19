using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.BaseValidators;

public class CountValidator : AbstractValidator<int>
{
	public CountValidator()
	{
		RuleFor(x => x)
			.GreaterThan(0)
			.WithLocalizableError(PositionCountMustBeGreaterThanZeroMessage.Instance);
	}
}
