namespace Application.Common.Interfaces.NamedObject;

public interface INamedObjectGroupRegistry
{
    Type GetRegistryType(string groupName);
}