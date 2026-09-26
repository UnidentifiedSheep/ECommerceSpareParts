using Locan.Core.Interfaces;

namespace NamedObject.Core.Interfaces;

public interface ILocalizableNamedObject : INamedObject
{
	ILocalizableMessage NameLocalizationMessage { get; }
	ILocalizableMessage DescriptionLocalizationMessage { get; }
}
