using Microsoft.Extensions.DependencyInjection;
using NamedObject.Core.Interfaces;

namespace NamedObject;

internal class NamedObjectGroupResolver(
	IEnumerable<INamedObjectGroupRegistry> registries,
	IServiceProvider serviceProvider)
	: INamedObjectGroupResolver
{
	public INamedObjectRegistry GetByGroupName(string groupName)
	{
		foreach (var registry in registries)
		{
			Type registryType;
			try
			{
				registryType = registry.GetRegistryType(groupName);
			}
			catch (KeyNotFoundException)
			{
				continue;
			}

			return (INamedObjectRegistry?)serviceProvider.GetRequiredService(registryType) ??
				throw new KeyNotFoundException($"Named object group '{groupName}' not found");
		}

		throw new KeyNotFoundException($"Named object group '{groupName}' not found");
	}
}
