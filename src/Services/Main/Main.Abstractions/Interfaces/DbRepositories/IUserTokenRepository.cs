using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserTokenRepository
{
    Task<UserToken?> GetTokenByHashAsync(
        string hash, 
        QueryOptions? options = null, 
        CancellationToken cancellationToken = default);
}