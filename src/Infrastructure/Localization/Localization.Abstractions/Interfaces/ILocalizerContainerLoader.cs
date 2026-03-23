namespace Localization.Abstractions.Interfaces;

public interface ILocalizerContainerLoader
{
    Task LoadAsync(IEnumerable<ILocalizerContainer> containers);
}