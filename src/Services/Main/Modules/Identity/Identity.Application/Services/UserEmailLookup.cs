using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using Main.Modules.Identity.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Identity.Application.Services;

public sealed class UserEmailLookup(IReadRepository<UserEmail, string> repository) : IUserEmailLookup
{
	public async Task<IReadOnlyDictionary<Guid, string>> GetPrimaryConfirmedEmailsAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default)
	{
		var emails = await repository.Query
			.AsNoTracking()
			.Where(email => userIds.Contains(email.UserId) && email.IsPrimary && email.Confirmed)
			.ToListAsync(cancellationToken);

		return emails
			.GroupBy(email => email.UserId)
			.ToDictionary(group => group.Key, group => group.First().Email.Value);
	}
}
