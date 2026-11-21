using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Product.Application.Queries.Producers.GetProducers;

public class GetProducersValidation : AbstractValidator<GetProducersQuery>
{
	public GetProducersValidation() => RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
