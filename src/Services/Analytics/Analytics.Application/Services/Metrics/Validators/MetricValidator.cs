using Analytics.Entities.Metrics;
using FluentValidation;

namespace Analytics.Application.Services.Metrics.Validators;

public class MetricValidator : AbstractValidator<Metric>
{
    public MetricValidator()
    {
        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("Created by is required");
        
        RuleFor(x => x.RangeStart)
            .NotEmpty()
            .WithMessage("Range start is required");
        
        RuleFor(x => x.RangeEnd)
            .NotEmpty()
            .WithMessage("Range end is required");
        
        RuleFor(x => x.CurrencyId)
            .NotEmpty()
            .WithMessage("Currency id is required");
        
        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeStart <= x.RangeEnd)
            .WithMessage("Range start must be less than or equal to the range end");
    }
}