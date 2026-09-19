using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Application.NamedObjects.StorageContentExtractPolicies;

public class FifoStorageContentExtractPolicy : StorageContentExtractPolicyBase
{
	public override string SystemName => "FifoStorageContentExtractPolicy";

	public override ILocalizableMessage NameLocalizationMessage =>
		FifoStorageContentExtractPolicyNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		FifoStorageContentExtractPolicyDescriptionMessage.Instance;

	public override IOrderedQueryable<StorageContent> Apply(IQueryable<StorageContent> query) =>
		query.OrderBy(x => x.PurchaseDatetime);
}
