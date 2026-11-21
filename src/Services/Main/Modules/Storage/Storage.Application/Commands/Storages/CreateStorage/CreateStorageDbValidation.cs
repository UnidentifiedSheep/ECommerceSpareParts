using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Storage;

namespace Main.Modules.Storage.Application.Commands.Storages.CreateStorage;

public class CreateStorageDbValidation : AbstractDbValidation<CreateStorageCommand>
{
	public override void Build(IValidationPlan plan, CreateStorageCommand request)
		=> plan.ValidateStorageNotExistsCode(request.Code.Trim());
}
