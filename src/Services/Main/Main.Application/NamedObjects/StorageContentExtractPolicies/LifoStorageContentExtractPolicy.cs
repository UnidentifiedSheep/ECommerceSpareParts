using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class LifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
	public override string SystemName => "LifoStorageContentExtractPolicy";
	public override ILocalizableMessage NameLocalizationMessage
		=> LifoStorageContentExtractPolicyNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LifoStorageContentExtractPolicyDescriptionMessage.Instance;

	public override IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query) =>
		query.OrderByDescending(x => x.PurchaseDatetime);
}
