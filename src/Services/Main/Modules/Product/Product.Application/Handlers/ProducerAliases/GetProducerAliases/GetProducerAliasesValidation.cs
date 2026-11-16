using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Product.Application.Handlers.ProducerAliases.GetProducerAliases;

public class GetProducerAliasesValidation : AbstractValidator<GetProducerAliasesQuery>
{
	public GetProducerAliasesValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
