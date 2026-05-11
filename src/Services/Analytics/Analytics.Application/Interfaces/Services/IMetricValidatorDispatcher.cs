namespace Analytics.Application.Interfaces.Services;

public interface IMetricValidatorDispatcher
{
    Task ValidateAsync(Type type, object metric, CancellationToken ct);
}