using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class LifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
    public override string SystemName { get; }
    protected override string NameLocalizationKey { get; }
    protected override string DescriptionLocalizationKey { get; }
    
    public override IQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
    {
        return query.OrderBy(x => x.PurchaseDatetime);
    }
}