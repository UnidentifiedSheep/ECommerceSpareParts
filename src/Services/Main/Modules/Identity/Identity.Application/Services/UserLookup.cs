using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Modules.Identity.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services;

public sealed class UserLookup(IReadRepository<User, Guid> repository, IUserRepository userRepository)
	: IUserLookup
{
	public async Task EnsureExistAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default)
	{
		var ids = userIds.Distinct().ToArray();
		if (ids.Length == 0)
			return;

		var existing = await repository.Query
			.Where(user => ids.Contains(user.Id))
			.Select(user => user.Id)
			.ToListAsync(cancellationToken);

		var missing = ids.Except(existing).FirstOrDefault();
		if (missing != Guid.Empty)
			throw new UserNotFoundException(missing);
	}

	public async Task<IReadOnlyList<UserMatch>> SearchAsync(
		string searchTerm,
		int limit,
		CancellationToken cancellationToken = default)
		=> await userRepository.SearchWithRank(repository.Query, searchTerm)
			.Take(limit)
			.Select(match => new UserMatch(match.User.Id, match.Rank))
			.ToListAsync(cancellationToken);
}
