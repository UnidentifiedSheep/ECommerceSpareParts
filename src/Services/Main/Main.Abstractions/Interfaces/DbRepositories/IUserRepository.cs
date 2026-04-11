using Abstractions.Models.Repository;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Entities;
using Main.Entities.User;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(
        QueryOptions<User, Guid> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetUsersBySimilarityAsync(
        QueryOptions<User, GetUsersBySimilarityOptionsData> options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetUserBySearchColumn(
        QueryOptions<User, GetUserBySearchColumnOptionsData> options,
        CancellationToken cancellationToken = default);

    Task<decimal?> GetUsersDiscountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ChangeUsersDiscount(Guid userId, decimal discount, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetUserInfo(
        QueryOptions<UserInfo, Guid> options,
        CancellationToken cancellationToken = default);
}