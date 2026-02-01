using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageRoutes.EditStorageRoute;

public class EditStorageRouteDbValidation : AbstractDbValidation<EditStorageRouteCommand>
{
    public override void Build(IValidationPlan plan, EditStorageRouteCommand request)
    {
        if (request.PatchStorageRoute.CarrierId is { IsSet: true, Value: not null })
            plan.ValidateUserExistsId(request.PatchStorageRoute.CarrierId.Value.Value);
    }
}