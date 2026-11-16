using Application.Common.Extensions;
using Domain;
using FluentValidation;

namespace Application.Common.Validators;

public class CountValidator : AbstractValidator<int>
{
	public CountValidator()
	{
		RuleFor(x => x)
			.GreaterThan(0)
			.WithLocalizableError(PositionCountMustBeGreaterThanZeroMessage.Instance);
	}
}
