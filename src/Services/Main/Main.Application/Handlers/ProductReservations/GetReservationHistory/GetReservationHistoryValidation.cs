using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.ProductReservations.GetReservationHistory;

public class GetReservationHistoryValidation : AbstractValidator<GetReservationHistoryQuery>
{
    public GetReservationHistoryValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}