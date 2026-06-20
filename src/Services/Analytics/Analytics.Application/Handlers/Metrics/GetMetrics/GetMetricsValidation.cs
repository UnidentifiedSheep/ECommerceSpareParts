using Application.Common.Validators;
using FluentValidation;

namespace Analytics.Application.Handlers.Metrics.GetMetrics;

public class GetMetricsValidation : AbstractValidator<GetMetricsQuery>
{
    public GetMetricsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}