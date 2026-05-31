using Application.Common.Validators;
using FluentValidation;

namespace Analytics.Application.Handlers.CalculationJob.GetCalculationJobs;

public class GetCalculationJobsValidation : AbstractValidator<GetCalculationJobsQuery>
{
    public GetCalculationJobsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}