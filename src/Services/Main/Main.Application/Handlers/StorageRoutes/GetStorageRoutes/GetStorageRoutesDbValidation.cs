using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRoutes;

public class GetStorageRoutesDbValidation : AbstractDbValidation<GetStorageRoutesQuery>
{
    public override void Build(IValidationPlan plan, GetStorageRoutesQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.StorageFrom))
            plan.ValidateStorageExistsName(request.StorageFrom);
        if (!string.IsNullOrWhiteSpace(request.StorageTo))
            plan.ValidateStorageExistsName(request.StorageTo);
    }
}