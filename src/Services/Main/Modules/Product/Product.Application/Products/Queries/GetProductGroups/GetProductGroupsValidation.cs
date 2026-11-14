using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Products.Queries.GetProductGroups;

public class GetProductGroupsValidation : AbstractValidator<GetProductGroupsQuery>
{
	public GetProductGroupsValidation()
	{
		RuleFor(query => query.Pagination)
			.SetValidator(new PaginationValidator());
	}
}
