using Main.Application.NamedObjects.StorageContentExtractPolicies;
using NamedObject.Core.Base;

namespace Main.Application.NamedObjects;

public class NamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
	public NamedObjectGroupRegistry()
	{
		Register<StorageContentExtractPolicyBase>("StorageContentExtractPolicy");
	}
}
