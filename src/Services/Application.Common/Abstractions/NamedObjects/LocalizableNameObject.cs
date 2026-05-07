using Application.Common.Interfaces;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Abstractions.NamedObjects;

public abstract class LocalizableNameObject : INamedObject
{
    public abstract string SystemName { get; }
    protected abstract string NameLocalizationKey { get; }
    protected abstract string DescriptionLocalizationKey { get; }

    public string GetLocalizedName(IScopedStringLocalizer stringLocalizer)
        => stringLocalizer.Get(NameLocalizationKey);
    
    public string GetLocalizedDescription(IScopedStringLocalizer stringLocalizer)
        => stringLocalizer.Get(DescriptionLocalizationKey);
}