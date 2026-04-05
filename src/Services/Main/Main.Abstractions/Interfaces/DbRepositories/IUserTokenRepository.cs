using Abstractions.Models.Repository;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserTokenRepository
{
    Task<UserToken?> GetTokenByHashAsync(
        QueryOptions<UserToken, string> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserToken>> GetTokensAsync(
        QueryOptions<UserToken, GetUserTokensOptionsData> queryOptions,
        CancellationToken cancellationToken = default);
}