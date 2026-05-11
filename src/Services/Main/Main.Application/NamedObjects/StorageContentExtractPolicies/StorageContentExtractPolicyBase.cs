using Application.Common.Abstractions.NamedObjects;
using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public abstract class StorageContentExtractPolicyBase : LocalizableNameObject
{
    public abstract IQueryable<StorageContent> Apply(IQueryable<StorageContent> query);
}