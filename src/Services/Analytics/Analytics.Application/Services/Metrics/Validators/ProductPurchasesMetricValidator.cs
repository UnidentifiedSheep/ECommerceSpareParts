using Analytics.Entities.Metrics;
using FluentValidation;

namespace Analytics.Application.Services.Metrics.Validators;

public class ProductPurchasesMetricValidator : AbstractValidator<ProductPurchasesMetric>
{
    public ProductPurchasesMetricValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product Id is required");

        RuleFor(x => x)
            .SetValidator(new MetricValidator());
    }
}