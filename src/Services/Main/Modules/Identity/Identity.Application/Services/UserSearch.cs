using Application.Common.Interfaces.Repositories;
using Application.Common.Querying;
using Main.Entities.User;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Identity.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Identity.Application.Services;

public sealed class UserSearch(IReadRepository<User, Guid> users, IUserRepository userRepository)
	: IUserSearch
{
	public IQueryable<Guid> MatchingIds(string term)
	{
		var pattern = LikePattern.Contains(term.Trim());

		return users.Query
			.Where(user => EF.Functions.ILike(
				user.UserName.Value,
				pattern,
				"\\"))
			.Select(user => user.Id);
	}

	public IQueryable<UserMatch> MatchWithRank(string term)
		=> userRepository
			.SearchWithRank(users.Query, term)
			.Select(match => new UserMatch
			{
				UserId = match.User.Id, Rank = match.Rank
			});
}
