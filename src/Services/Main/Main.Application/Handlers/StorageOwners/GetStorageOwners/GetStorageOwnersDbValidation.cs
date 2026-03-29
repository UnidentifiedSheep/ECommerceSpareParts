using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageOwners.GetStorageOwners;

public class GetStorageOwnersDbValidation : AbstractDbValidation<GetStorageOwnersQuery>
{
    public override void Build(IValidationPlan plan, GetStorageOwnersQuery request)
    {
        plan.ValidateStorageExistsName(request.Name);
    }
}