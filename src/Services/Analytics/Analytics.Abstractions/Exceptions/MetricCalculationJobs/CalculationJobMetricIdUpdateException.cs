using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Analytics.Abstractions.Exceptions.MetricCalculationJobs;

public class CalculationJobMetricIdUpdateException() 
    : BadRequestException(null), ILocalizableException
{
    public string MessageKey => "metric.calculation.job.metric.id.set.once";
    public object[]? Arguments => null;
}