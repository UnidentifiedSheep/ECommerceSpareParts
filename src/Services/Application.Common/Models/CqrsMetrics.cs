using System.Diagnostics.Metrics;

namespace Application.Common.Models;

public sealed class CqrsMetrics
{
    public const string MeterName = "Application.Cqrs";
    public Histogram<double> Duration { get; }
    public Counter<long> Requests { get; }
    public Counter<long> Errors { get; }

    public CqrsMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        Duration = meter.CreateHistogram<double>(
            "cqrs.request.duration",
            unit: "ms",
            description: "CQRS request execution duration");

        Requests = meter.CreateCounter<long>(
            "cqrs.request.count",
            unit: "{request}",
            description: "Number of CQRS requests");

        Errors = meter.CreateCounter<long>(
            "cqrs.request.errors",
            unit: "{error}",
            description: "Number of failed CQRS requests");
    }
}