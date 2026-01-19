using BulkValidation.Core.Interfaces;
using BulkValidation.Core.Models;
using Core.Interfaces;
using Main.Persistence.Context;
using Npgsql;

namespace Main.Persistence;

public class PgsqlDbValidator(BulkValidation.Base.Interfaces.IDbValidator<DContext, NpgsqlParameter> validator) : IDbValidator
{
    public async Task<IEnumerable<ValidationFailure>> Validate(IValidationPlan plan, bool throwOnError = true, CancellationToken cancellationToken = default)
        => await validator.Validate(plan, throwOnError, cancellationToken);
}