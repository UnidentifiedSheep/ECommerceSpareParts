using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Analytics.Abstractions.Exceptions.Metrics;

public class MetricNotFoundException(Guid id) 
    : NotFoundException(null, new { Id = id }), ILocalizableException
{
    public string MessageKey => "metric.not.found";
    public object[]? Arguments => null;
}