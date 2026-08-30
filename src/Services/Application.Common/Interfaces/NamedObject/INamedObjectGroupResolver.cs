namespace Application.Common.Interfaces.NamedObject;

public interface INamedObjectGroupResolver
{
	INamedObjectRegistry GetByGroupName(string groupName);
}
