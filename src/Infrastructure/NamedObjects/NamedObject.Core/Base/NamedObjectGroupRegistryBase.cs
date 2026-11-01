using NamedObject.Core.Interfaces;

namespace NamedObject.Core.Base;

public abstract class NamedObjectGroupRegistryBase : INamedObjectGroupRegistry
{
	private readonly Dictionary<string, Type> _map = new();
	public IReadOnlyDictionary<string, Type> Map => _map;

	public Type GetRegistryType(string groupName)
	{
		return _map.TryGetValue(groupName, out var type)
			? type
			: throw new KeyNotFoundException(
				$"Named object group '{groupName}' not found");
	}

	protected void Register<TNamedObjectBase>(string groupName)
		where TNamedObjectBase : class, INamedObject
	{
		Register(groupName, typeof(INamedObjectRegistry<TNamedObjectBase>));
	}

	protected void Register(string groupName, Type registryType)
	{
		if (!_map.TryAdd(groupName, registryType))
			throw new InvalidOperationException(
				$"Named object group '{groupName}' is already registered");
	}
}
