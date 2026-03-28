using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(
        Guid userId,
        QueryOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetUsersBySimilarityAsync(
        double similarityLevel,
        PageableQueryOptions<User> options,
        string? name = null,
        string? surname = null,
        string? email = null,
        string? phone = null,
        string? userName = null,
        Guid? id = null,
        string? description = null,
        bool? isSupplier = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetUserBySearchColumn(
        string? searchTerm,
        bool? isSupplier = null,
        PageableQueryOptions<User>? options = null,
        CancellationToken cancellationToken = default);

    Task<decimal?> GetUsersDiscountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ChangeUsersDiscount(Guid userId, decimal discount, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetUserInfo(Guid id, QueryOptions? options = null, CancellationToken cancellationToken = default);
}