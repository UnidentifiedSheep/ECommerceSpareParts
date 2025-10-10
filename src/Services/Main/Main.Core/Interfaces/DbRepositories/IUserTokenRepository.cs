using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IUserTokenRepository
{
    Task<UserToken?> GetTokenByIdAsync(Guid id, bool track = true, CancellationToken cancellationToken = default);
    Task<UserToken?> GetTokenByHashAsync(string hash, bool track = true, CancellationToken cancellationToken = default);
    Task<bool> TokenExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> TokenExistsAsync(string hash, CancellationToken cancellationToken = default);
}