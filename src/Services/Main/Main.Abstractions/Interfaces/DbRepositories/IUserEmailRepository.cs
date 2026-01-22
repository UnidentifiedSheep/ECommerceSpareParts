using Main.Abstractions.Models;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserEmailRepository
{
    Task<IEnumerable<UserEmail>> GetUserEmailsAsync(Guid userId, int? limit = null, int? offset = null,
        bool track = true, CancellationToken cancellationToken = default);

    Task<User?> GetUserByPrimaryMailAsync(string email, bool track = true,
        CancellationToken cancellationToken = default);
}