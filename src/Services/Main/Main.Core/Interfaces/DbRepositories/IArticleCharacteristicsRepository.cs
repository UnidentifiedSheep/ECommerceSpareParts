using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IArticleCharacteristicsRepository
{
    Task<IEnumerable<ArticleCharacteristic>> GetArticleCharacteristics(int articleId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ArticleCharacteristic>> GetArticleCharacteristicsByIds(int? articleId, IEnumerable<int> ids,
        bool track = true, CancellationToken cancellationToken = default);

    Task<ArticleCharacteristic?> GetCharacteristic(int id, bool track = true,
        CancellationToken cancellationToken = default);
}