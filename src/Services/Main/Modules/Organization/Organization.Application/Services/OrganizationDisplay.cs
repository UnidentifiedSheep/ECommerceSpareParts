using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Modules.Organization.Abstractions;
using Main.Modules.Organization.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Organization.Application.Services;

public sealed class OrganizationDisplay(
	IReadRepository<Entities.Organization.Organization.Organization, Guid> organizations)
	: IDisplay<Guid, OrganizationDisplayInfo>
{
	public async Task<IReadOnlyDictionary<Guid, OrganizationDisplayInfo>> GetAsync(
		IReadOnlyCollection<Guid> ids,
		CancellationToken cancellationToken = default)
		=> await organizations.Query
			.Where(organization => ids.Contains(organization.Id))
			.ToDictionaryAsync(
				organization => organization.Id,
				organization => new OrganizationDisplayInfo(organization.Name),
				cancellationToken);
}
