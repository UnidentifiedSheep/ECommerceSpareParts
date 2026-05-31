using Analytics.Entities.Metrics;
using FluentValidation;

namespace Analytics.Application.Services.Metrics.Validators;

public class ProductSalesMetricValidator : AbstractValidator<ProductSalesMetric>
{
    public ProductSalesMetricValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product Id is required");

        RuleFor(x => x)
            .SetValidator(new MetricValidator());
    }
}