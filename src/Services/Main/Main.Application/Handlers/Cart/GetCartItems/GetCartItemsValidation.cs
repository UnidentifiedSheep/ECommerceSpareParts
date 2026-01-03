using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Cart.GetCartItems;

public class GetCartItemsValidation : AbstractValidator<GetCartItemsQuery>
{
    public GetCartItemsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}