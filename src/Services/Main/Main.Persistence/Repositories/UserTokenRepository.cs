using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserTokenRepository(DContext context) : IUserTokenRepository
{
    public async Task<UserToken?> GetTokenByIdAsync(Guid id, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.UserTokens.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserToken?> GetTokenByHashAsync(string hash, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.UserTokens.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
    }

    public async Task<bool> TokenExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.UserTokens.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> TokenExistsAsync(string hash, CancellationToken cancellationToken = default)
    {
        return await context.UserTokens.AsNoTracking().AnyAsync(x => x.TokenHash == hash, cancellationToken);
    }
}