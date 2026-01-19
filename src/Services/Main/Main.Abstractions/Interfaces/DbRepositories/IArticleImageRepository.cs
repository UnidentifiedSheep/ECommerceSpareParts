using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleImageRepository
{
    Task<IEnumerable<ArticleImage>> GetArticlesImages(IEnumerable<int> articleIds, bool track = true,
        CancellationToken cancellationToken = default);
}