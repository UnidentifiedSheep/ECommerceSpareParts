using Locan.Core.Interfaces;
using Main.Entities.Storage;
using Main.Entities.Storage.Storage;

namespace Main.Modules.Storage.Application.NamedObjects.StorageContentExtractPolicies;

public class LifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
	public override string SystemName => "LifoStorageContentExtractPolicy";

	public override ILocalizableMessage NameLocalizationMessage
		=> LifoStorageContentExtractPolicyNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LifoStorageContentExtractPolicyDescriptionMessage.Instance;

	public override IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
		=> query.OrderByDescending(x => x.PurchaseDatetime);
}
