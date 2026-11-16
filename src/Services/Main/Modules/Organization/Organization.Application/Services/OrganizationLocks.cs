using Application.Common.Interfaces.Repositories;
using Main.Modules.Organization.Abstractions;

namespace Main.Modules.Organization.Application.Services;

public sealed class OrganizationLocks(IRepository<Entities.Organization.Organization.Organization, Guid> repository) : IOrganizationLocks
{
	public async Task LockExistingAsync(
		IReadOnlyCollection<Guid> organizationIds,
		CancellationToken cancellationToken = default)
	{
		var ids = organizationIds.ToArray();
		var organizations = await repository.ListAsync(
			Criteria<Entities.Organization.Organization.Organization>
				.New()
				.Where(x => ids.Contains(x.Id))
				.OrderByAsc(x => x.Id)
				.ForUpdate()
				.Track()
				.Build(),
			cancellationToken);

		if (organizations.Count != ids.Distinct().Count() || ids.Distinct().Count() != ids.Length)
			throw new InvalidOperationException("Locked organization does not exist");
	}
}
