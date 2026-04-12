using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;

public class SubtractCountFromReservationsDbValidation : AbstractDbValidation<SubtractCountFromReservationsCommand>
{
    public override void Build(IValidationPlan plan, SubtractCountFromReservationsCommand request)
    {
        plan.ValidateUserExistsId(request.UserId)
            .ValidateUserExistsId(request.WhoUpdated);
    }
}