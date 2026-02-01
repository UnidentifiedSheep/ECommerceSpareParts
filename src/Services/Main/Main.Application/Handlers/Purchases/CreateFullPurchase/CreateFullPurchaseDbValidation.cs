using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

public class CreateFullPurchaseDbValidation : AbstractDbValidation<CreateFullPurchaseCommand>
{
    public override void Build(IValidationPlan plan, CreateFullPurchaseCommand request)
    {
        if (request is { WithLogistics: true, StorageFrom: not null })
            plan.ValidateStorageOwnerExistsPK((request.StorageFrom, request.SupplierId));
        
    }
}