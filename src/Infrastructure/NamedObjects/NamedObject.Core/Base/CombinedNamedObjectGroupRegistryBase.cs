namespace NamedObject.Core.Base;

public abstract class CombinedNamedObjectGroupRegistryBase : NamedObjectGroupRegistryBase
{
	protected void FromGroupRegistry(NamedObjectGroupRegistryBase groupRegistry)
	{
		foreach (var (key, type) in groupRegistry.Map)
			Register(key, type);
	}
}
