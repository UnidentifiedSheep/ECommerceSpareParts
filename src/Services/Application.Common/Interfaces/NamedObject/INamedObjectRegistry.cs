namespace Application.Common.Interfaces.NamedObject;

public interface INamedObjectRegistry<TBaseObject> : INamedObjectRegistry where TBaseObject : INamedObject
{
    new IReadOnlyCollection<TBaseObject> All { get; }
    new TBaseObject GetBySystemName(string systemName);
    new TBaseObject? TryGetBySystemName(string systemName);
}

public interface INamedObjectRegistry
{
    IReadOnlyCollection<INamedObject> All { get; }
    INamedObject GetBySystemName(string systemName);
    INamedObject? TryGetBySystemName(string systemName);
}