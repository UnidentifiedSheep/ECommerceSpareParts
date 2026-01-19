using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.DeleteContent;

public class DeleteStorageContentDbValidation : AbstractDbValidation<DeleteStorageContentCommand>
{
    public override void Build(IValidationPlan plan, DeleteStorageContentCommand request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}