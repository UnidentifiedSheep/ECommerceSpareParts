using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Organizations;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Organization;
using Main.Entities.User;
using Main.Enums.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Organizations.GetOrganizations;

public record GetOrganizationsQuery(
	Pagination Pagination,
	IReadOnlyCollection<string> SortBy,
	string? SearchTerm,
	Guid? UserId,
	IReadOnlyCollection<Guid> Ids,
	IReadOnlyCollection<OrganizationType> Types,
	bool ShowHidden) : IQuery<GetOrganizationsResult>;

public record GetOrganizationsResult(IReadOnlyList<OrganizationListItemDto> Organizations);

public class GetOrganizationsHandler(
	IReadRepository<Organization, Guid> repository,
	IUserRepository userRepository,
	IReadRepository<User, Guid> userReadRepository,
	IProjectionProvider<Organization, OrganizationListItemDto> projection)
	: IQueryHandler<GetOrganizationsQuery, GetOrganizationsResult>
{
	public async Task<GetOrganizationsResult> Handle(
		GetOrganizationsQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query.Where(x => x.Type != OrganizationType.System);

		if (!request.ShowHidden)
			query = query.Where(x => !x.IsHidden);

		if (request.Ids.Count > 0)
			query = query.Where(x => request.Ids.Contains(x.Id));

		if (request.Types.Count > 0)
			query = query.Where(x => request.Types.Contains(x.Type));

		if (request.UserId.HasValue)
			query = query.Where(x => x.Members.Any(member => member.UserId == request.UserId.Value));

		var searchTerm = request.SearchTerm?.Trim();
		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			var pattern = $"%{searchTerm}%";
			var matchingUsers = userRepository.SearchWithRank(userReadRepository.Query, searchTerm);
			query = query
				.Select(x => new
				{
					Organization = x,
					OrganizationRank =
						EF.Functions.TrigramsSimilarity(x.Name, searchTerm) +
						EF.Functions.TrigramsSimilarity(x.SystemName, searchTerm),
					MemberRank = matchingUsers
						.Where(match => x.Members.Any(member => member.UserId == match.User.Id))
						.Select(match => (float?)match.Rank)
						.Max()
				})
				.Where(x =>
					EF.Functions.ILike(x.Organization.Name, pattern) ||
					EF.Functions.ILike(x.Organization.SystemName, pattern) || x.MemberRank.HasValue ||
					x.OrganizationRank >= 0.3)
				.OrderByDescending(x => x.OrganizationRank + (x.MemberRank ?? 0))
				.ThenBy(x => x.Organization.Id)
				.Select(x => x.Organization);
		}

		if (request.SortBy.Count > 0 || string.IsNullOrWhiteSpace(searchTerm))
			query = query.SortBy(request.SortBy);

		var organizations = await query
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetOrganizationsResult(organizations);
	}
}
