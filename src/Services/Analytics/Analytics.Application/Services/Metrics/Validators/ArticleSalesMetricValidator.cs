using Analytics.Entities.Metrics;
using FluentValidation;

namespace Analytics.Application.Services.Metrics.Validators;

public class ArticleSalesMetricValidator : AbstractValidator<ArticleSalesMetric>
{
    public ArticleSalesMetricValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty()
            .WithMessage("Article Id is required");

        RuleFor(x => x)
            .SetValidator(new MetricValidator());
    }
}