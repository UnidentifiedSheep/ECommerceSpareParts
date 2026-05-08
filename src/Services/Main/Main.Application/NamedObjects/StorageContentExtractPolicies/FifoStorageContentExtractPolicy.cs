using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class FifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
    public override string SystemName => "FifoStorageContentExtractPolicy";
    protected override string NameLocalizationKey => "fifo.storage.content.extract.policy.name";
    protected override string DescriptionLocalizationKey => "fifo.storage.content.extract.policy.description";

    public override IQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
    {
        return query.OrderBy(x => x.PurchaseDatetime);
    }
}