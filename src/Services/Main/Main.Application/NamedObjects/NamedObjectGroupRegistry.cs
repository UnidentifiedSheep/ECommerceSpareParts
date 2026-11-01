using Main.Application.Analytics;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using NamedObject.Core.Base;

namespace Main.Application.NamedObjects;

public class NamedObjectGroupRegistry : CombinedNamedObjectGroupRegistryBase
{
	public NamedObjectGroupRegistry()
	{
		FromGroupRegistry(new AnalyticsNamedObjectGroupRegistry());
		Register<StorageContentExtractPolicyBase>("StorageContentExtractPolicy");
	}
}
