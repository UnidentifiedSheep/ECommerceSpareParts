using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

public class CreatePurchaseDbValidation : AbstractDbValidation<CreatePurchaseCommand>
{
    public override void Build(IValidationPlan plan, CreatePurchaseCommand request)
    {
        if (request is { WithLogistics: true, StorageFrom: not null })
            plan.ValidateStorageOwnerExistsPK((request.StorageFrom, request.SupplierId));
    }
}