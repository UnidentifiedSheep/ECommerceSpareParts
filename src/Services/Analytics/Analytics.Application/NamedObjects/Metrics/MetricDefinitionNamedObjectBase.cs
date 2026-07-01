using Analytics.Application.NamedObjects.Metrics.MetricInputBases;
using Analytics.Application.NamedObjects.Metrics.MetricInputValidators;
using Analytics.Entities.Exceptions;
using Analytics.Entities.Metrics;
using Application.Common.Abstractions.NamedObjects;
using Extensions;

namespace Analytics.Application.NamedObjects.Metrics;

public abstract class MetricDefinitionNamedObjectBase<TMetric, TInput>
    : MetricDefinitionNamedObjectBase<TMetric>
    where TMetric : Metric
    where TInput : MetricInputBase
{
    public override Type InputType => typeof(TInput);

    public override TMetric CreateMetric(string json)
    {
        var input = ValidateInput(json);
        return CreateMetric(input);
    }

    protected abstract TMetric CreateMetric(TInput input);

    protected virtual TInput ValidateInput(string json)
    {
        if (!json.TryDeserializeJson<TInput>(out var value)) throw new MetricInvalidInputException();

        var result = new MetricInputBaseValidator().Validate(value);

        return !result.IsValid ? throw new MetricInvalidInputException() : value;
    }
}

public abstract class MetricDefinitionNamedObjectBase<TMetric>
    : MetricDefinitionNamedObjectBase where TMetric : Metric
{
    public override Type MetricType => typeof(TMetric);

    public abstract TMetric CreateMetric(string json);

    public override Metric CreateMetricUntyped(string json) { return CreateMetric(json); }
}

public abstract class MetricDefinitionNamedObjectBase : LocalizableNameObject
{
    public abstract Type MetricType { get; }
    public abstract Type InputType { get; }

    public abstract Metric CreateMetricUntyped(string json);

    protected static TMetric FillMetricBase<TMetric>(
        TMetric metric,
        MetricInputBase input)
        where TMetric : Metric
    {
        metric.ConfigurePeriod(
            input.CurrencyId,
            input.RangeStart,
            input.RangeEnd);
        return metric;
    }
}