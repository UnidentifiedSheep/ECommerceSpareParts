using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleWeightRepository
{
    Task<ProductWeight?> GetArticleWeight(int articleId, bool track = true, CancellationToken token = default);

    Task<IEnumerable<ProductWeight>> GetArticleWeightsByIds(
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken token = default);
}