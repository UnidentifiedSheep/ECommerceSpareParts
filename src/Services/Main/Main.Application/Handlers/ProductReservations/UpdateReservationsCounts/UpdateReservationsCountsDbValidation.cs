using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.UpdateReservationsCounts;

public class UpdateReservationsCountsDbValidation : AbstractDbValidation<UpdateReservationsCountsCommand>
{
    public override void Build(IValidationPlan plan, UpdateReservationsCountsCommand request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}