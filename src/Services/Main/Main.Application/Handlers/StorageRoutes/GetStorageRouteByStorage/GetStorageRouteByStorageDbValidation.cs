using Application.Common.Abstractions;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRouteByStorage;

public class GetStorageRouteByStorageDbValidation : AbstractDbValidation<GetStorageRouteByStorageQuery>
{
    public override void Build(IValidationPlan plan, GetStorageRouteByStorageQuery request)
    {
        plan.ValidateStorageExistsName(Quantifier.All, request.StorageFrom, request.StorageTo);
    }
}