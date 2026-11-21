using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Product;

namespace Main.Modules.Product.Application.Commands.ProductEnrichment.CreateCandidateCrosses;

public sealed class CreateCandidateCrossesValidation : AbstractValidator<CreateCandidateCrossesCommand>
{
	public CreateCandidateCrossesValidation()
	{
		RuleFor(x => x.CrossCandidateIds)
			.NotEmpty()
			.WithLocalizableError(CatalogueCandidateCrossesNotEmptyMessage.Instance);
	}
}
