using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class FirstCheapestStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
    public override string SystemName { get; }
    protected override string NameLocalizationKey { get; }
    protected override string DescriptionLocalizationKey { get; }
    
    public override IQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
    {
        return query
            .Select(x => new
            {
                x,
                PriceInUsd =
                    x.Currency.CurrencyToUsd == null
                        ? (decimal?)null
                        : x.BuyPrice * x.Currency.CurrencyToUsd.ToUsd
            })
            .OrderBy(x => x.PriceInUsd == null)
            .ThenBy(x => x.PriceInUsd)
            .Select(x => x.x);//TODO
    }
}