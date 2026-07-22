using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductReservations.UpdateOrganizationReservationCounts;

public class UpdateOrganizationReservationCountsDbValidation
    : AbstractDbValidation<UpdateOrganizationReservationCountsCommand>
{
    public override void Build(
        IValidationPlan plan,
        UpdateOrganizationReservationCountsCommand request)
    {
        plan.ValidateOrganizationExistsId(request.OrganizationId);
    }
}
