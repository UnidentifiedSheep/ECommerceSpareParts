using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.CreateProductReservation;

public class CreateProductReservationDbValidation : AbstractDbValidation<CreateProductReservationCommand>
{
    public override void Build(IValidationPlan plan, CreateProductReservationCommand request)
    {
        var articleIds = new HashSet<int>();
        var userIds = new HashSet<Guid>();

        foreach (var item in request.Reservations)
        {
            articleIds.Add(item.ProductId);
            userIds.Add(item.UserId);
        }

        plan.ValidateProductExistsId(articleIds)
            .ValidateUserExistsId(userIds);
    }
}