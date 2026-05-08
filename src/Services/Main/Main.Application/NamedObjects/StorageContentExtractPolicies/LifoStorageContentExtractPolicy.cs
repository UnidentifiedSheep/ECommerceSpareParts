using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class LifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
    public override string SystemName => "LifoStorageContentExtractPolicy";
    protected override string NameLocalizationKey => "lifo.storage.content.extract.policy.name";
    protected override string DescriptionLocalizationKey => "lifo.storage.content.extract.policy.description";

    public override IQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
    {
        return query.OrderBy(x => x.PurchaseDatetime);
    }
}