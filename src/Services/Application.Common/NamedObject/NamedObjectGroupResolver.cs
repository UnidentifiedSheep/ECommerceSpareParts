using Application.Common.Interfaces.NamedObject;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.NamedObject;

public class NamedObjectGroupResolver(
    INamedObjectGroupRegistry registry,
    IServiceProvider serviceProvider) : INamedObjectGroupResolver
{
    public INamedObjectRegistry GetByGroupName(string groupName)
    {
        var registryType = registry.GetRegistryType(groupName);

        return (INamedObjectRegistry?)serviceProvider.GetRequiredService(registryType)
               ?? throw new KeyNotFoundException($"Named object group '{groupName}' not found");
    }
}