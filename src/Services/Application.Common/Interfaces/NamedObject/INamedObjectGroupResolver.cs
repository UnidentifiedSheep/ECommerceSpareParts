namespace Application.Common.Interfaces.NamedObject;

public interface INamedObjectGroupResolver
{
    public INamedObjectRegistry GetByGroupName(string groupName);
}