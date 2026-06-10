using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class FirstCheapestStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
    public override string SystemName => "FirstCheapestStorageContentExtractPolicy";
    public override string NameLocalizationKey => "first.cheapest.storage.content.extract.policy.name";
    public override string DescriptionLocalizationKey => "first.cheapest.storage.content.extract.policy.description";

    public override IQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
    {
        return query.OrderBy(x => x.BuyPriceInBaseCurrency);
    }
}