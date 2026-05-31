using Analytics.Entities.Metrics;
using Analytics.Enums;
using Bogus;
using Test.Common.Abstractions;

namespace Analytics.Integration.Tests.DataBuilders.Base;

public abstract class MetricDataBuilderBase<T, TMetric>(Faker faker) 
    : BuilderBase<TMetric>(faker) 
    where T : MetricDataBuilderBase<T, TMetric>
    where TMetric : Metric
{
    public int? CurrencyId { get; private set; }
    public DateTime? Start { get; private set; }
    public DateTime? End { get; private set; }
    public RecalculationTags? Tags { get; private set; }
    public bool Recalculated { get; private set; }
    
    public T WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return (T)this;
    }

    public T WithStartDate(DateTime start)
    {
        Start = start;
        return (T)this;
    }

    public T WithEndDate(DateTime end)
    {
        End = end;
        return (T)this;
    }

    public T WithRecalculated(bool recalculated)
    {
        Recalculated = recalculated;
        return (T)this;
    }

    public T WithRecalculationTags(RecalculationTags tags)
    {
        Tags = tags;
        return (T)this;
    }

    protected void FillBase(Metric metric)
    {
        metric.ConfigurePeriod(
            CurrencyId ?? Faker.Random.Int(1),
            Start ?? Faker.Date.Past(),
            End ?? Faker.Date.Future());

        if (Tags.HasValue)
        {
            if(Tags.Value.HasFlag(RecalculationTags.Disabled))
                metric.Disable();
            if (Tags.Value.HasFlag(RecalculationTags.RecalculationNeeded))
                metric.MarkDirty();
        }

        if (Recalculated)
            metric.CompleteRecalculation();
    }
}