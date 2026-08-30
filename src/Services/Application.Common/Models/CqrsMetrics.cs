using System.Diagnostics.Metrics;

namespace Application.Common.Models;

public sealed class CqrsMetrics
{
	public const string MeterName = "Application.Cqrs";

	public CqrsMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(MeterName);

		Duration = meter.CreateHistogram<double>(
			"cqrs.request.duration",
			"ms",
			"CQRS request execution duration");

		Requests = meter.CreateCounter<long>(
			"cqrs.request.count",
			"{request}",
			"Number of CQRS requests");

		Errors = meter.CreateCounter<long>(
			"cqrs.request.errors",
			"{error}",
			"Number of failed CQRS requests");
	}

	public Histogram<double> Duration { get; }

	public Counter<long> Requests { get; }

	public Counter<long> Errors { get; }
}
