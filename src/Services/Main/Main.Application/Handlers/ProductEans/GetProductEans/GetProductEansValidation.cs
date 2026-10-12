using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.ProductEans.GetProductEans;

public class GetProductEansValidation : AbstractValidator<GetProductEansQuery>
{
	public GetProductEansValidation()
	{
		RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
	}
}
