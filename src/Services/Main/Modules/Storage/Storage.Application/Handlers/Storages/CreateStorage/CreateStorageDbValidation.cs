using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.Handlers.Storages.CreateStorage;

public class CreateStorageDbValidation : AbstractDbValidation<CreateStorageCommand>
{
	public override void Build(IValidationPlan plan, CreateStorageCommand request)
		=> plan.ValidateStorageNotExistsCode(request.Code.Trim());
}
