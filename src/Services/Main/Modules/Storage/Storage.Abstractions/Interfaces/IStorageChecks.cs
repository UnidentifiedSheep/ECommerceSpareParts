using BulkValidation.Core.Interfaces;

namespace Main.Modules.Storage.Abstractions;

public interface IStorageChecks
{
	IValidationPlan Exists(IValidationPlan plan, string storageCode);
}
