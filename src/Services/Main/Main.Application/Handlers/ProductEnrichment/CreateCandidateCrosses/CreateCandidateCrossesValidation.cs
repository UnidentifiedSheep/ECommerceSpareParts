using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.ProductEnrichment.CreateCandidateCrosses;

public sealed class CreateCandidateCrossesValidation : AbstractValidator<CreateCandidateCrossesCommand>
{
	public CreateCandidateCrossesValidation()
	{
		RuleFor(x => x.CrossCandidateIds)
			.NotEmpty()
			.WithLocalizableError(CatalogueCandidateCrossesNotEmptyMessage.Instance);
	}
}
