using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Jobs.Application.Handlers.Jobs.GetJobs;

public class GetJobsValidation : AbstractValidator<GetJobsQuery>
{
	public GetJobsValidation() => RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
