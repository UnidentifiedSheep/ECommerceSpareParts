using Sannr;

namespace Application.Common.Aot.Interfaces;

public interface IValidation<in TRequest>
{
    Task<ValidationResult> ValidateAsync(TRequest request);
}