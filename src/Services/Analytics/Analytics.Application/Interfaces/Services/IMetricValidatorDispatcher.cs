namespace Analytics.Abstractions.Interfaces.Application;

public interface IMetricValidatorDispatcher
{
    Task ValidateAsync(Type type, object metric, CancellationToken ct);
}