using Main.Entities.Storage.Storage;
using NamedObject.Core.Base;

namespace Main.Modules.Storage.Application.NamedObjects.StorageContentExtractPolicies;

public abstract class StorageContentExtractPolicyBase : LocalizableNameObject
{
	public abstract IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query);
}
