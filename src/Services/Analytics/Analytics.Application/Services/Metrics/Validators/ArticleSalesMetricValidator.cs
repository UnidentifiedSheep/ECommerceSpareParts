using Analytics.Entities.Metrics;
using FluentValidation;

namespace Analytics.Application.Services.Metrics.Validators;

public class ArticleSalesMetricValidator : AbstractValidator<ProductSalesMetric>
{
    public ArticleSalesMetricValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Article Id is required");

        RuleFor(x => x)
            .SetValidator(new MetricValidator());
    }
}