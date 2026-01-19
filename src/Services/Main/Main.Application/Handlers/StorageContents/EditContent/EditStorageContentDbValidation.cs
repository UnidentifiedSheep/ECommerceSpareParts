using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.EditContent;

public class EditStorageContentDbValidation : AbstractDbValidation<EditStorageContentCommand>
{
    public override void Build(IValidationPlan plan, EditStorageContentCommand request)
    {
        plan.ValidateUserExistsId(request.UserId);
    }
}