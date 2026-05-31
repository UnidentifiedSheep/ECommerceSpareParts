using FluentValidation;

namespace Analytics.Application.Interfaces.Services.Metrics;

public interface IMetricValidatorDispatcher
{
    Task ValidateAsync(Type type, object metric, CancellationToken ct);
    IValidator? GetValidator(Type metricType);
}