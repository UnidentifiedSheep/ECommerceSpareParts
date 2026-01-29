using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageRoutes.AddStorageRoute;

public class AddStorageRouteDbValidation : AbstractDbValidation<AddStorageRouteCommand>
{
    public override void Build(IValidationPlan plan, AddStorageRouteCommand request)
    {
        plan.ValidateStorageExistsName(request.StorageTo)
            .ValidateStorageExistsName(request.StorageFrom);
    }
}