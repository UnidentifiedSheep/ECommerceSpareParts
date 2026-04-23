using Abstractions.Models.Repository;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Dtos.RepositoryOptionsData;
using Main.Entities;
using Main.Entities.Auth;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserTokenRepository(DContext context) : IUserTokenRepository
{
    public async Task<UserToken?> GetTokenByHashAsync(
        QueryOptions<UserToken, string> options,
        CancellationToken cancellationToken = default)
    {
        return await context.UserTokens
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.TokenHash == options.Data, cancellationToken);
    }

    public async Task<IReadOnlyList<UserToken>> GetTokensAsync(
        QueryOptions<UserToken, GetUserTokensOptionsData> queryOptions, 
        CancellationToken cancellationToken = default)
    {
        var query = context.UserTokens
            .ApplyOptions(queryOptions)
            .Where(x => x.UserId == queryOptions.Data.UserId);

        if (queryOptions.Data.TokenType != null)
            query = query.Where(x => x.Type == queryOptions.Data.TokenType);
        
        return await query.ApplyPaging(queryOptions).ToListAsync(cancellationToken);
    }
}