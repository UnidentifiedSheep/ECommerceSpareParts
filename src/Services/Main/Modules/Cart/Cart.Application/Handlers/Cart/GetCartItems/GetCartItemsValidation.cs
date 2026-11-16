using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Cart.Application.Handlers.Cart.GetCartItems;

public class GetCartItemsValidation : AbstractValidator<GetCartItemsQuery>
{
	public GetCartItemsValidation() => RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
