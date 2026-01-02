using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface ICartRepository
{
    Task<Cart?> GetCartItemAsync(Guid userId, int articleId, bool track = true,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Cart>> GetUserCartAsync(Guid userId, bool track = true, int? page = null, int? limit = null, 
        CancellationToken cancellationToken = default);
}