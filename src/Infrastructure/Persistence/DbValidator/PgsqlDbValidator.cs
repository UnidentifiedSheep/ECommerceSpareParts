using Abstractions.Interfaces;
using BulkValidation.Core.Interfaces;
using BulkValidation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace Persistence.DbValidator;

public sealed class PgsqlDbValidator<TContext>(BulkValidation.Base.Interfaces.IDbValidator<TContext, NpgsqlParameter> validator)
    : IDbValidator
    where TContext : DbContext
{
    public async Task<IEnumerable<ValidationFailure>> Validate(IValidationPlan plan, bool throwOnError = true, 
        CancellationToken cancellationToken = default)
        => await validator.Validate(plan, throwOnError, cancellationToken);
}