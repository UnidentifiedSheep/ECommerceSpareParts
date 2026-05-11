using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.EditProductReservation;

public class EditProductReservationDbValidation : AbstractDbValidation<EditProductReservationCommand>
{
    public override void Build(IValidationPlan plan, EditProductReservationCommand request)
    {
        if (request.NewValue.GivenCurrencyId.HasValue)
            plan.ValidateCurrencyExistsId(request.NewValue.GivenCurrencyId.Value);
    }
}