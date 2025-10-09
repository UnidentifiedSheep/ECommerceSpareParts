using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;

namespace Main.Application.Services;

public class ArticlesService(IArticlesRepository articlesRepository) : IArticlesService
{
    public async Task UpdateArticlesCount(Dictionary<int, int> toUpdate, CancellationToken cancellationToken = default)
    {
        var expectedCount = toUpdate.Count;
        var updatedRows = await articlesRepository.UpdateArticlesCount(toUpdate, cancellationToken);

        if (updatedRows != expectedCount) throw new InvalidOperationException("Не все артикулы найдены для обновления");
    }
}