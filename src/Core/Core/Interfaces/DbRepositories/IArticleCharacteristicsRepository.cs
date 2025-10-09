using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IArticleCharacteristicsRepository
{
    Task<IEnumerable<ArticleCharacteristic>> GetArticleCharacteristics(int articleId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<ArticleCharacteristic?> GetCharacteristic(int id, bool track = true,
        CancellationToken cancellationToken = default);
}