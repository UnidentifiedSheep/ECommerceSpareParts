using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.MoveContentToOtherStorage;

public class MoveContentToOtherStorageDbValidation : AbstractDbValidation<MoveContentToOtherStorageCommand>
{
    public override void Build(IValidationPlan plan, MoveContentToOtherStorageCommand request)
    {
        plan.ValidateStorageExistsName(request.Movements.Select(x => x.NewStorageName).ToHashSet());
    }
}