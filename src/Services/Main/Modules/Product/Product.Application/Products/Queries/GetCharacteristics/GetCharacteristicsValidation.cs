using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Product.Application.Products.Queries.GetCharacteristics;

public class GetCharacteristicsValidation : AbstractValidator<GetCharacteristicsQuery>
{
	public GetCharacteristicsValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
