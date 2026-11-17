using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using Main.Modules.Identity.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Identity.Application.Services;

public sealed class UserNames(IReadRepository<User, Guid> users) : IUserNames
{
	public async Task<IReadOnlyDictionary<Guid, string?>> GetFullNamesAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default)
		=> (await users.Query
				.Where(user => userIds.Contains(user.Id))
				.Select(user => new
				{
					user.Id,
					FullName = user.UserInfo == null ? null : user.UserInfo.Name + " " + user.UserInfo.Surname
				})
				.ToListAsync(cancellationToken))
			.ToDictionary(item => item.Id, item => item.FullName);
}
