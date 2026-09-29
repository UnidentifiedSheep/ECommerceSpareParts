using BulkValidation.Core.Interfaces;
using BulkValidation.Core.Models;

namespace Application.Common.Interfaces.Persistence;

public interface IDbValidator
{
	Task<IEnumerable<ValidationFailure>> Validate(
		IValidationPlan plan,
		bool throwOnError = true,
		CancellationToken cancellationToken = default);
}
