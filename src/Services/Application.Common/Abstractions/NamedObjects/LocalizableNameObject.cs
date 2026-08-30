using Application.Common.Interfaces.NamedObject;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Abstractions.NamedObjects;

public abstract class LocalizableNameObject : ILocalizableNamedObject
{
	public abstract string NameLocalizationKey { get; }

	public abstract string DescriptionLocalizationKey { get; }

	public abstract string SystemName { get; }

	public string GetLocalizedName(IContextualStringLocalizer stringLocalizer) =>
		stringLocalizer.Get(NameLocalizationKey);

	public string GetLocalizedDescription(IContextualStringLocalizer stringLocalizer) =>
		stringLocalizer.Get(DescriptionLocalizationKey);
}
