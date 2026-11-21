using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Product.Application.Queries.Characteristics.GetCharacteristics;

public class GetCharacteristicsValidation : AbstractValidator<GetCharacteristicsQuery>
{
	public GetCharacteristicsValidation()
	{
		RuleFor(query => query.Pagination)
			.SetValidator(new PaginationValidator());
	}
}
