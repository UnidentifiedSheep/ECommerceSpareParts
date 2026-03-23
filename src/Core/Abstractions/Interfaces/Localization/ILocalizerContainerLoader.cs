namespace Abstractions.Interfaces.Localization;

public interface ILocalizerContainerLoader
{
    Task LoadAsync(IEnumerable<ILocalizerContainer> containers);
}