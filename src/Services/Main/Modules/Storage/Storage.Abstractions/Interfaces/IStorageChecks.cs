using BulkValidation.Core.Interfaces;

namespace Main.Modules.Storage.Abstractions.Interfaces;

public interface IStorageChecks
{
	IValidationPlan Exists(IValidationPlan plan, string storageCode);
}
