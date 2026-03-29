using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserEmailRepository
{
    Task<IReadOnlyList<UserEmail>> GetUserEmailsAsync(
        QueryOptions<UserEmail, Guid> options,
        CancellationToken cancellationToken = default);

    Task<UserEmail?> GetUserEmailByPrimary(
        QueryOptions<UserEmail, string> options,
        CancellationToken cancellationToken = default);
}