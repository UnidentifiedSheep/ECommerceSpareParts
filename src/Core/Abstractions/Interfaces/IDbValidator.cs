using BulkValidation.Core.Interfaces;
using BulkValidation.Core.Models;

namespace Abstractions.Interfaces;

public interface IDbValidator
{
    Task<IEnumerable<ValidationFailure>> Validate(IValidationPlan plan, bool throwOnError = true,
        CancellationToken cancellationToken = default);
}