using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.ProductGroups.GetProductGroups;

public class GetProductGroupsValidation : AbstractValidator<GetProductGroupsQuery>
{
	public GetProductGroupsValidation()
	{
		RuleFor(query => query.Pagination)
			.SetValidator(new PaginationValidator());
	}
}
