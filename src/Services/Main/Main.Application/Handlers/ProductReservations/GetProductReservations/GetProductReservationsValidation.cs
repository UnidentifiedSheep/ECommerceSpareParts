using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.ProductReservations.GetProductReservations;

public class GetProductReservationsValidation : AbstractValidator<GetProductReservationsQuery>
{
    public GetProductReservationsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}