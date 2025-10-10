using Main.Core.Entities;
using Main.Core.Models;

namespace Main.Core.Interfaces.DbRepositories;

public interface IUserEmailRepository
{
    Task<IEnumerable<UserEmail>> GetUserEmailsAsync(Guid userId, int? limit = null, int? offset = null,
        bool track = true, CancellationToken cancellationToken = default);

    Task<UserEmail?> GetUserEmailAsync(Guid id, bool track = true,
        CancellationToken cancellationToken = default);

    Task<UserEmail?> GetUserEmailAsync(string email, bool track = true,
        CancellationToken cancellationToken = default);

    Task<User?> GetUserByPrimaryMailAsync(string email, bool track = true,
        CancellationToken cancellationToken = default);

    Task<UserEmail?> GetUserPrimaryEmailAsync(Guid userId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default);
    Task<int> GetUserEmailCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UserHasPrimaryEmailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserEmailSummary?> GetUserEmailSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExists(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> IsEmailsExists(IEnumerable<string> emails, CancellationToken cancellationToken = default);
}