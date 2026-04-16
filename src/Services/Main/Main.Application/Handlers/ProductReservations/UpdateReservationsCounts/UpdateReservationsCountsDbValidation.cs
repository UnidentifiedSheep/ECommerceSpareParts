using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;

public class UpdateReservationsCountsDbValidation : AbstractDbValidation<UpdateReservationsCountsCommand>
{
    public override void Build(IValidationPlan plan, UpdateReservationsCountsCommand request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}