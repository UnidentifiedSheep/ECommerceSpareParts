using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserTokenRepository(DContext context) : IUserTokenRepository
{
    public async Task<UserToken?> GetTokenByHashAsync(
        string hash,
        QueryOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        return await context.UserTokens
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
    }
}