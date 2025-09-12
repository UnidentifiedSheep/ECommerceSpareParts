namespace Core.Interfaces;

public interface IMarkupGenerator
{
    Task ReCalculateMarkupAsync(CancellationToken cancellationToken = default);
}