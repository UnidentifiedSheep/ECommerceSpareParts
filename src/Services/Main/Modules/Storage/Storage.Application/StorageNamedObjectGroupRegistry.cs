using Main.Modules.Storage.Application.NamedObjects.StorageContentExtractPolicies;
using NamedObject.Core.Base;

namespace Main.Modules.Storage.Application;

public sealed class StorageNamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
	public StorageNamedObjectGroupRegistry()
		=> Register<StorageContentExtractPolicyBase>("StorageContentExtractPolicy");
}
