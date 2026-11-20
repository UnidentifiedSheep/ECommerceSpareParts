using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Product.Application.Queries.Products.GetProductGroups;

public class GetProductGroupsValidation : AbstractValidator<GetProductGroupsQuery>
{
	public GetProductGroupsValidation()
	{
		RuleFor(query => query.Pagination)
			.SetValidator(new PaginationValidator());
	}
}
