using Main.Entities;
using Main.Entities.Product;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleContentRepository
{
    Task<IEnumerable<ProductContent>> GetArticleContents(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<ProductContent?> GetArticleContent(
        int articleId,
        int insideArticleId,
        bool track = true,
        CancellationToken cancellationToken = default);
}