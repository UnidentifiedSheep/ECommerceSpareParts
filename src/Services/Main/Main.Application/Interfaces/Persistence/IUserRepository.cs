using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Main.Entities.User;

namespace Main.Application.Interfaces.Persistence;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<decimal?> GetUsersDiscountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByPrimaryEmailAsync(
        string email, 
        Criteria<User>? criteria = null, 
        CancellationToken cancellationToken = default);
}