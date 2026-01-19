using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface ICartRepository
{
    Task<Cart?> GetCartItemAsync(Guid userId, int articleId, bool track = true,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Cart>> GetCartItemsAsync(Guid userId, bool track = true, int? page = null, int? limit = null, 
        CancellationToken cancellationToken = default);
}