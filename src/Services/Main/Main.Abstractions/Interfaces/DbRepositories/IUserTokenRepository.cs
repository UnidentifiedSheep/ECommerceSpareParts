using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserTokenRepository
{
    Task<UserToken?> GetTokenByHashAsync(
        QueryOptions<UserToken, string> options,
        CancellationToken cancellationToken = default);
}