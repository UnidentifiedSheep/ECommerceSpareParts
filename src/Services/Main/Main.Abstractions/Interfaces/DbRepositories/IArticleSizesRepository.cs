using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleSizesRepository
{
    Task<ArticleSize?> GetArticleSizes(int articleId, bool track = true, CancellationToken token = default);
}