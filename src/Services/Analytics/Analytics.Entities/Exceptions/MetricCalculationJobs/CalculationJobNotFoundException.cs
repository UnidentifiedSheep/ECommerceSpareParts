using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Analytics.Abstractions.Exceptions.MetricCalculationJobs;

public class CalculationJobNotFoundException(Guid requestId)
    : NotFoundException(null, new { RequestId = requestId }), ILocalizableException
{
    public string MessageKey => "metric.calculation.job.not.found";
    public object[]? Arguments => null;
}