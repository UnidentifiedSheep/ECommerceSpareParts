namespace NamedObject.Core.Interfaces;

public interface INamedObjectGroupResolver
{
	INamedObjectRegistry GetByGroupName(string groupName);
}
