using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleWeightRepository
{
    Task<ArticleWeight?> GetArticleWeight(int articleId, bool track = true, CancellationToken token = default);
}