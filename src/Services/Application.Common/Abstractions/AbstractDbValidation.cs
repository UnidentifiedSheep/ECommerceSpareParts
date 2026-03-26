using BulkValidation.Core.Interfaces;

namespace Application.Common.Abstractions;

public abstract class AbstractDbValidation<TRequest>
{
    public abstract void Build(IValidationPlan plan, TRequest request);
}