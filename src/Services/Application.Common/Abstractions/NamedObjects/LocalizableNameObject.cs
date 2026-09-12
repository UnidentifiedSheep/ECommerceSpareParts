using Application.Common.Interfaces.NamedObject;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;

namespace Application.Common.Abstractions.NamedObjects;

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
