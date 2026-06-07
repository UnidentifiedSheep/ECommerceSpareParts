using Application.Common.Interfaces.NamedObject;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Abstractions.NamedObjects;

public abstract class LocalizableNameObject : ILocalizableNamedObject
{
    public abstract string NameLocalizationKey { get; }
    public abstract string DescriptionLocalizationKey { get; }
    public abstract string SystemName { get; }

    public string GetLocalizedName(IScopedStringLocalizer stringLocalizer)
    {
        return stringLocalizer.Get(NameLocalizationKey);
    }

    public string GetLocalizedDescription(IScopedStringLocalizer stringLocalizer)
    {
        return stringLocalizer.Get(DescriptionLocalizationKey);
    }
}