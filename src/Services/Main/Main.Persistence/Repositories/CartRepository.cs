using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class CartRepository(DContext context) : ICartRepository
{
    public async Task<Cart?> GetCartItemAsync(Guid userId, int articleId, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.Carts
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ArticleId == articleId, cancellationToken);
    }

    public async Task<IEnumerable<Cart>> GetCartItemsAsync(Guid userId, bool track = true, int? page = null, int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Carts
            .Include(x => x.Article)
            .ThenInclude(x => x.Producer)
            .ConfigureTracking(track)
            .Where(x => x.UserId == userId);

        if (page is >= 0 && limit is > 0)
            query = query.Skip(page.Value * limit.Value).Take(limit.Value);

        return await query.ToListAsync(cancellationToken);
    }
}