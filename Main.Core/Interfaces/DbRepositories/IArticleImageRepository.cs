using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IArticleImageRepository
{
    Task<IEnumerable<ArticleImage>> GetArticlesImages(IEnumerable<int> articleIds, bool track = true,
        CancellationToken cancellationToken = default);
}