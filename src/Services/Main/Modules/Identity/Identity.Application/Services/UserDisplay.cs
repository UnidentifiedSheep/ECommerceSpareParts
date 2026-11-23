using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using Main.Modules.Identity.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Identity.Application.Services;

public sealed class UserDisplay(IReadRepository<User, Guid> users) : IDisplay<Guid, UserDisplayInfo>
{
	public async Task<IReadOnlyDictionary<Guid, UserDisplayInfo>> GetAsync(
		IReadOnlyCollection<Guid> ids,
		CancellationToken cancellationToken = default)
		=> (await users.Query
				.Where(user => ids.Contains(user.Id))
				.Select(user => new
				{
					user.Id,
					FullName = user.UserInfo == null ? null : user.UserInfo.Name + " " + user.UserInfo.Surname
				})
				.ToListAsync(cancellationToken))
			.ToDictionary(item => item.Id, item => new UserDisplayInfo(item.FullName));
}
