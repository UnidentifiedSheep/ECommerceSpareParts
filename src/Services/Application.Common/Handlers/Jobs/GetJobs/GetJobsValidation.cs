using Application.Common.Validators;
using FluentValidation;

namespace Application.Common.Handlers.Jobs.GetJobs;

public class GetJobsValidation : AbstractValidator<GetJobsQuery>
{
    public GetJobsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}