using Main.Entities.Storage;
using NamedObject.Core.Base;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public abstract class StorageContentExtractPolicyBase : LocalizableNameObject
{
	public abstract IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query);
}
