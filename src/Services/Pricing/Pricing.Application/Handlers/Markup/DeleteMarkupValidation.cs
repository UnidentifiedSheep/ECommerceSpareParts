using Application.Common.Extensions;
using FluentValidation;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Markup;

public sealed class DeleteMarkupValidation : AbstractValidator<DeleteMarkupCommand>
{
	public DeleteMarkupValidation()
	{
		RuleFor(x => x.Id).GreaterThan(0)
			.WithLocalizableError(MarkupGroupIdMustBePositiveMessage.Instance);
	}
}
