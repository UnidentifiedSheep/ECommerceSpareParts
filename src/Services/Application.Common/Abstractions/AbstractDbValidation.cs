using BulkValidation.Core.Interfaces;
using BulkValidation.Core.Plan;

namespace Application.Common.Abstractions;

public abstract class AbstractDbValidation<TRequest>
{
    public abstract void Build(IValidationPlan plan, TRequest request);
}