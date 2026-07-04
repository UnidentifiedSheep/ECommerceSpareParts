namespace Application.Common.Interfaces.NamedObject;

public interface ILocalizableNamedObject : INamedObject
{
    string NameLocalizationKey { get; }
    string DescriptionLocalizationKey { get; }
}