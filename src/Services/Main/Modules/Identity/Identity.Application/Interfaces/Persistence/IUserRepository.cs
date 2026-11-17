using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using Main.Modules.Identity.Application.Models.Users;

namespace Main.Modules.Identity.Application.Interfaces.Persistence;

public interface IUserRepository : IRepository<User, Guid>
{
	Task RevokeRefreshTokensAsync(
		Guid userId,
		string reason,
		CancellationToken cancellationToken = default);

	IQueryable<User> Search(IQueryable<User> query, string searchTerm);

	IQueryable<UserSearchMatch> SearchWithRank(IQueryable<User> query, string searchTerm);

	Task<User?> GetUserByPrimaryEmailAsync(
		string email,
		Criteria<User>? criteria = null,
		CancellationToken cancellationToken = default);

	Task<User?> GetUserByLoginAsync(
		string login,
		bool isEmail,
		Criteria<User>? criteria = null,
		CancellationToken cancellationToken = default);
}
