using BulkValidation.Core.Interfaces;
using Main.Entities.Storage;
using Main.Modules.Storage.Abstractions;

namespace Main.Application.Services.Storage;

public sealed class StorageChecks : IStorageChecks
{
	public IValidationPlan Exists(IValidationPlan plan, string storageCode) => plan.ValidateStorageExistsCode(storageCode);
}
