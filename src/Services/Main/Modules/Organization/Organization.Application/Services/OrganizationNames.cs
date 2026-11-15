using Application.Common.Interfaces.Repositories;
using Main.Entities.Organization;
using Main.Modules.Organization.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services;

public sealed class OrganizationNames(IReadRepository<Organization, Guid> organizations) : IOrganizationNames
{
	public async Task<IReadOnlyDictionary<Guid, string>> GetAsync(
		IReadOnlyCollection<Guid> organizationIds,
		CancellationToken cancellationToken = default)
		=> await organizations.Query
			.Where(organization => organizationIds.Contains(organization.Id))
			.ToDictionaryAsync(organization => organization.Id, organization => organization.Name, cancellationToken);
}
