using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Storages.CreateStorage;

public class CreateStorageDbValidation : AbstractDbValidation<CreateStorageCommand>
{
    public override void Build(IValidationPlan plan, CreateStorageCommand request)
    {
        plan.ValidateStorageNotExistsName(request.Name.Trim());
    }
}