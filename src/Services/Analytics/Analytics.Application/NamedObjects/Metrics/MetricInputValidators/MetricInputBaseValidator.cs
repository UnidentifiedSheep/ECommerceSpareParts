using Analytics.Application.NamedObjects.Metrics.MetricInputBases;
using FluentValidation;
using Localization.Domain.Extensions;

namespace Analytics.Application.NamedObjects.Metrics.MetricInputValidators;

public class MetricInputBaseValidator : AbstractValidator<MetricInputBase>
{
    public MetricInputBaseValidator()
    {
        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeStart <= x.RangeEnd)
            .WithLocalizationKey("metric.input.range.start.must.be.before.or.equal.end");
    }
}
