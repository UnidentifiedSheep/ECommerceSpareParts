using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class FirstCheapestStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
	public override string SystemName => "FirstCheapestStorageContentExtractPolicy";

	public override ILocalizableMessage NameLocalizationMessage =>
		FirstCheapestStorageContentExtractPolicyNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		FirstCheapestStorageContentExtractPolicyDescriptionMessage.Instance;

	public override IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query) =>
		query.OrderBy(x => x.BuyPriceInBaseCurrency);
}
