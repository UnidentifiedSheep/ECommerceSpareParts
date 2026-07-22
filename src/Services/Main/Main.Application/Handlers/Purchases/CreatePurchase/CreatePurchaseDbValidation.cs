using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

public class CreatePurchaseDbValidation : AbstractDbValidation<CreatePurchaseCommand>
{
    public override void Build(IValidationPlan plan, CreatePurchaseCommand request)
    {
        plan.ValidateUserExistsId(request.SupplierUserId)
            .ValidateOrganizationExistsId(request.SupplierOrganizationId)
            .ValidateOrganizationMemberExistsPK(
                (request.SupplierOrganizationId, request.SupplierUserId));

        if (request is { WithLogistics: true, StorageFrom: not null })
            plan.ValidateStorageOwnerExistsPK((request.StorageFrom, request.SupplierUserId));
    }
}
