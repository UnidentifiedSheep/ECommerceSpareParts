using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class FifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
    public override string SystemName => "FifoStorageContentExtractPolicy";
    public override string NameLocalizationKey => "fifo.storage.content.extract.policy.name";
    public override string DescriptionLocalizationKey => "fifo.storage.content.extract.policy.description";

    public override IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
    {
        return query.OrderBy(x => x.PurchaseDatetime);
    }
}