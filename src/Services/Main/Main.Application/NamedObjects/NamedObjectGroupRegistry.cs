using Application.Common.Abstractions.NamedObjects;
using Main.Application.NamedObjects.StorageContentExtractPolicies;

namespace Main.Application.NamedObjects;

public class NamedObjectGroupRegistry : NamedObjectGroupRegistryBase
{
    public NamedObjectGroupRegistry()
    {
        Register<StorageContentExtractPolicyBase>("StorageContentExtractPolicy");
    }
}