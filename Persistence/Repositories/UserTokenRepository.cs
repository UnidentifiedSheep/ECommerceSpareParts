using Core.Entities;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class UserTokenRepository(DContext context) : IUserTokenRepository
{
    public async Task<UserToken?> GetTokenByIdAsync(Guid id, bool track = true, CancellationToken cancellationToken = default) 
        => await context.UserTokens.ConfigureTracking(track).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<UserToken?> GetTokenByHashAsync(string hash, bool track = true, CancellationToken cancellationToken = default) 
        => await context.UserTokens.ConfigureTracking(track).FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

    public async Task<bool> TokenExistsAsync(Guid id, CancellationToken cancellationToken = default) 
        => await context.UserTokens.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> TokenExistsAsync(string hash, CancellationToken cancellationToken = default)
        => await context.UserTokens.AsNoTracking().AnyAsync(x => x.TokenHash == hash, cancellationToken);
}