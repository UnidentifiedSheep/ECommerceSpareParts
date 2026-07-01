using Application.Common.Interfaces.NamedObject;

namespace Application.Common.NamedObject;

public class NamedObjectRegistry<TBaseObject>
    : INamedObjectRegistry<TBaseObject>
    where TBaseObject : class, INamedObject
{
    private readonly IReadOnlyDictionary<string, TBaseObject> _objects;

    public NamedObjectRegistry(IEnumerable<TBaseObject> objects)
    {
        _objects = objects.ToDictionary(
            x => x.SystemName,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<TBaseObject> All => _objects.Values.ToList();


    public TBaseObject GetBySystemName(string systemName)
    {
        return !_objects.TryGetValue(systemName, out var obj)
            ? throw new KeyNotFoundException($"Named object '{systemName}' not found")
            : obj;
    }

    public TBaseObject? TryGetBySystemName(string systemName)
    {
        _objects.TryGetValue(systemName, out var obj);
        return obj;
    }

    INamedObject? INamedObjectRegistry.TryGetBySystemName(string systemName)
    {
        return TryGetBySystemName(systemName);
    }

    IReadOnlyCollection<INamedObject> INamedObjectRegistry.All => All;

    INamedObject INamedObjectRegistry.GetBySystemName(string systemName)
    {
        return GetBySystemName(systemName);
    }
}