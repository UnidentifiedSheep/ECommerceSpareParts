using Locan.Core.Interfaces;

namespace Application.Common.Interfaces.NamedObject;

public interface ILocalizableNamedObject : INamedObject
{
	ILocalizableMessage NameLocalizationMessage { get; }
	ILocalizableMessage DescriptionLocalizationMessage { get; }
}
