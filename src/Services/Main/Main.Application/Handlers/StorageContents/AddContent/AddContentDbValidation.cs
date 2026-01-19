using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.AddContent;

public class AddContentDbValidation : AbstractDbValidation<AddContentCommand>
{
    public override void Build(IValidationPlan plan, AddContentCommand request)
    {
        plan.ValidateStorageExistsName(request.StorageName)
            .ValidateUserExistsId(request.UserId);
    }
}