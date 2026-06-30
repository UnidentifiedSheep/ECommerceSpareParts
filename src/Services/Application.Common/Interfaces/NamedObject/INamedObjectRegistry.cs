namespace Application.Common.Interfaces.NamedObject;

public interface INamedObjectRegistry<TBaseObject> : INamedObjectRegistry where TBaseObject : INamedObject
{
    new TBaseObject GetBySystemName(string systemName);
    new TBaseObject? TryGetBySystemName(string systemName);
    new IReadOnlyCollection<TBaseObject> All { get; }
}

public interface INamedObjectRegistry
{
    INamedObject GetBySystemName(string systemName);
    INamedObject? TryGetBySystemName(string systemName);
    IReadOnlyCollection<INamedObject> All { get; }
}