namespace Main.Core.Interfaces.Services;

public interface IArticlesService
{
    Task UpdateArticlesCount(Dictionary<int, int> toUpdate, CancellationToken cancellationToken = default);
}