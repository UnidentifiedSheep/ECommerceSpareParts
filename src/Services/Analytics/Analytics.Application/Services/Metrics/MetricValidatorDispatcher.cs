using Analytics.Abstractions.Interfaces.Application;
using FluentValidation;

namespace Analytics.Application.Services.Metrics;

public class MetricValidatorDispatcher(IServiceProvider provider) : IMetricValidatorDispatcher
{
    public async Task ValidateAsync(Type type, object metric, CancellationToken ct)
    {
        var validator = GetValidator(type);
        ArgumentNullException.ThrowIfNull(validator);
        
        var context = new ValidationContext<object>(metric);
        var result = await validator.ValidateAsync(context, ct);
        
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }
    
    private IValidator? GetValidator(Type metricType)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(metricType);
        var validator = provider.GetService(validatorType) as IValidator;
        return validator;
    }
}