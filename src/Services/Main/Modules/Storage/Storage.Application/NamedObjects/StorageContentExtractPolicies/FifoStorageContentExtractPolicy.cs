using Locan.Core.Interfaces;
using Main.Entities.Storage;
using Main.Entities.Storage.Storage;

namespace Main.Modules.Storage.Application.NamedObjects.StorageContentExtractPolicies;

public class FifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
	public override string SystemName => "FifoStorageContentExtractPolicy";

	public override ILocalizableMessage NameLocalizationMessage
		=> FifoStorageContentExtractPolicyNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> FifoStorageContentExtractPolicyDescriptionMessage.Instance;

	public override IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query)
		=> query.OrderBy(x => x.PurchaseDatetime);
}
