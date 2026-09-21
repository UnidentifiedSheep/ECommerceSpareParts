using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using NamedObject.Core.Interfaces;

namespace NamedObject.Core.Base;

public abstract class LocalizableNameObject : ILocalizableNamedObject
{
	public abstract string SystemName { get; }
	public abstract ILocalizableMessage NameLocalizationMessage { get; }
	public abstract ILocalizableMessage DescriptionLocalizationMessage { get; }

	public string GetLocalizedName(IContextualLocalizer stringLocalizer) =>
		stringLocalizer.Get(NameLocalizationMessage);

	public string GetLocalizedDescription(IContextualLocalizer stringLocalizer) =>
		stringLocalizer.Get(DescriptionLocalizationMessage);
}
