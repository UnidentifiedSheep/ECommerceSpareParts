using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserEmailRepository
{
    Task<IReadOnlyList<UserEmail>> GetUserEmailsAsync(
        Guid userId,
        PageableQueryOptions<UserEmail>? options = null,
        CancellationToken cancellationToken = default);

    Task<UserEmail?> GetPrimaryUserEmail(
        string email,
        QueryOptions<UserEmail>? options = null,
        CancellationToken cancellationToken = default);
}