using Application.Common.Interfaces.NamedObject;

namespace Application.Common.Abstractions.NamedObjects;

public abstract class NamedObjectGroupRegistryBase : INamedObjectGroupRegistry
{
    private readonly Dictionary<string, Type> _map = new();
    
    protected void Register<TNamedObjectBase>(string groupName) where TNamedObjectBase : class, INamedObject
    {
        var serviceType = typeof(INamedObjectRegistry<>).MakeGenericType(typeof(TNamedObjectBase));
        _map[groupName] = serviceType;
    }
    
    public Type GetRegistryType(string groupName)
    {
        return _map.TryGetValue(groupName, out var type) 
            ? type 
            : throw new KeyNotFoundException($"Named object group '{groupName}' not found");
    }
}