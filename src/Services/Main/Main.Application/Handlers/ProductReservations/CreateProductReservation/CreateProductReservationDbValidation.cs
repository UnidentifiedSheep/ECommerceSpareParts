using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.CreateProductReservation;

public class CreateProductReservationDbValidation : AbstractDbValidation<CreateProductReservationCommand>
{
    public override void Build(IValidationPlan plan, CreateProductReservationCommand request)
    {
        var reservation = request.Reservation;

        plan.ValidateProductExistsId(reservation.ProductId)
            .ValidateOrganizationExistsId(reservation.OrganizationId);

        if (reservation.GivenCurrencyId.HasValue)
            plan.ValidateCurrencyExistsId(reservation.GivenCurrencyId.Value);
    }
}
