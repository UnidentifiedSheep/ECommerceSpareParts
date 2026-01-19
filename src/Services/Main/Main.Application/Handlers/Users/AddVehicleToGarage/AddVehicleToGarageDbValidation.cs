using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Users.AddVehicleToGarage;

public class AddVehicleToGarageDbValidation : AbstractDbValidation<AddVehicleToGarageCommand>
{
    public override void Build(IValidationPlan plan, AddVehicleToGarageCommand request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}