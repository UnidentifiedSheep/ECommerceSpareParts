using Application.Common.Interfaces.Repositories;
using Application.Common.Querying;
using Main.Entities.Organization;
using Main.Modules.Organization.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services;

public sealed class OrganizationSearch(IReadRepository<Organization, Guid> organizations) : IOrganizationSearch
{
	public IQueryable<Guid> MatchingIds(string term)
	{
		var pattern = LikePattern.Contains(term.Trim());

		return organizations.Query
			.Where(organization => EF.Functions.ILike(organization.Name, pattern, "\\"))
			.Select(organization => organization.Id);
	}
}
