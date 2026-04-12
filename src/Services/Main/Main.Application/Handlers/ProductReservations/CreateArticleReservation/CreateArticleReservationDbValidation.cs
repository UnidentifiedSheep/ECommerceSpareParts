using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ArticleReservations.CreateArticleReservation;

public class CreateArticleReservationDbValidation : AbstractDbValidation<CreateArticleReservationCommand>
{
    public override void Build(IValidationPlan plan, CreateArticleReservationCommand request)
    {
        var articleIds = new HashSet<int>();
        var userIds = new HashSet<Guid> { request.WhoCreated };

        foreach (var item in request.Reservations)
        {
            articleIds.Add(item.ArticleId);
            userIds.Add(item.UserId);
        }

        plan.ValidateArticleExistsId(articleIds)
            .ValidateUserExistsId(userIds);
    }
}