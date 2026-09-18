using Application.Common;
using Application.Common.Extensions;
using FluentValidation;

namespace Main.Application.Handlers.ProductEnrichment.CreateCandidateCrosses;

public sealed class CreateCandidateCrossesValidation : AbstractValidator<CreateCandidateCrossesCommand>
{
	public CreateCandidateCrossesValidation()
	{
		RuleFor(x => x.RightCandidateIds)
			.NotEmpty()
			.WithLocalizableError(EmptyMessage.Instance); //TODO: create message.
	}
}
