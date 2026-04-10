using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleCharacteristicsRepository
{
    Task<IEnumerable<ProductCharacteristic>> GetArticleCharacteristics(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductCharacteristic>> GetArticleCharacteristicsByIds(
        int? articleId,
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken cancellationToken = default);

    Task<ProductCharacteristic?> GetCharacteristic(
        int id,
        bool track = true,
        CancellationToken cancellationToken = default);
}