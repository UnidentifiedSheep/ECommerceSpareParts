using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.RemoveContent;

public class RemoveContentDbValidation : AbstractDbValidation<RemoveContentCommand>
{
    public override void Build(IValidationPlan plan, RemoveContentCommand request)
    {
        plan.ValidateUserExistsId(request.UserId);
        
        if (!request.TakeFromOtherStorages && !string.IsNullOrWhiteSpace(request.StorageName))
            plan.ValidateStorageExistsName(request.StorageName);
    }
}