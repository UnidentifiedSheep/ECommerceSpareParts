using Application.Common.Validators;
using FluentValidation;

namespace Application.Common.Handlers.JobSchedules.GetSchedule;

public class GetScheduleValidation : AbstractValidator<GetScheduleQuery>
{
    public GetScheduleValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}