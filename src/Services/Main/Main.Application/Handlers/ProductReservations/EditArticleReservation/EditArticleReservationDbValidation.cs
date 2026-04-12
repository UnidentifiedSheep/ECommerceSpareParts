using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleReservations.EditArticleReservation;

public class EditArticleReservationDbValidation : AbstractDbValidation<EditArticleReservationCommand>
{
    public override void Build(IValidationPlan plan, EditArticleReservationCommand request)
    {
        plan.ValidateArticleExistsId(request.NewValue.ArticleId);
    }
}