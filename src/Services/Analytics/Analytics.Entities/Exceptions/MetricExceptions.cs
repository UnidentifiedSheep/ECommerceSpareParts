using Exceptions.Base.Localized;

namespace Analytics.Entities.Exceptions;

public class MetricNotFoundException : LocalizedNotFoundException
{
    private const string Key = "metric.not.found";

    public MetricNotFoundException(Guid id)
        : base(Key, new { Id = id })
    {
    }

    public MetricNotFoundException() : base(Key) { }
}

public class MetricInvalidInputException()
    : LocalizedBadRequestException("metric.input.invalid");