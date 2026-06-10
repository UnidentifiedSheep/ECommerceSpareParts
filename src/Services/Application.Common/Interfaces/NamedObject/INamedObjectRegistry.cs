namespace Application.Common.Interfaces.NamedObject;

public interface INamedObjectRegistry<TBaseObject> where TBaseObject : INamedObject
{
    TBaseObject GetBySystemName(string systemName);
    TBaseObject? TryGetBySystemName(string systemName);
    IReadOnlyCollection<TBaseObject> All { get; }
}