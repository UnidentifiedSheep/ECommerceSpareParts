using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Jobs.Application.Handlers.JobSchedules.GetSchedule;

public class GetScheduleValidation : AbstractValidator<GetScheduleQuery>
{
	public GetScheduleValidation() => RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
