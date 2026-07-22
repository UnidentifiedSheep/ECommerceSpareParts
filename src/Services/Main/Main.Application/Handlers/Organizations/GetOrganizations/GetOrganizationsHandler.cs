using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Organizations;
using Main.Application.Projections;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Organizations.GetOrganizations;

public record GetOrganizationsQuery(
    Pagination Pagination,
    string? SortBy,
    string? SearchTerm,
    IReadOnlyCollection<Guid> Ids,
    IReadOnlyCollection<OrganizationType> Types
) : IQuery<GetOrganizationsResult>;

public record GetOrganizationsResult(IReadOnlyList<OrganizationDto> Organizations);

public class GetOrganizationsHandler(IReadRepository<Organization, Guid> repository)
    : IQueryHandler<GetOrganizationsQuery, GetOrganizationsResult>
{
    public async Task<GetOrganizationsResult> Handle(
        GetOrganizationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.Ids.Count > 0)
            query = query.Where(x => request.Ids.Contains(x.Id));

        if (request.Types.Count > 0)
            query = query.Where(x => request.Types.Contains(x.Type));

        var searchTerm = request.SearchTerm?.Trim();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query
                .Select(x => new
                {
                    Organization = x,
                    OrganizationRank =
                        EF.Functions.TrigramsSimilarity(x.Name, searchTerm) +
                        EF.Functions.TrigramsSimilarity(x.SystemName, searchTerm),
                    MemberRank = x.Members
                        .Where(member => member.User.UserInfo != null)
                        .Select(member =>
                            EF.Functions.TrigramsSimilarity(
                                member.User.UserInfo!.SearchColumn,
                                searchTerm) +
                            EF.Functions.TrigramsWordSimilarity(
                                member.User.UserInfo!.SearchColumn,
                                searchTerm) * 0.7)
                        .OrderByDescending(rank => rank)
                        .FirstOrDefault()
                })
                .Where(x => x.MemberRank >= 0.3 || x.OrganizationRank >= 0.3)
                .OrderByDescending(x => x.OrganizationRank + x.MemberRank)
                .Select(x => x.Organization);

        var organizations = await query
            .SortBy(request.SortBy)
            .AsExpandable()
            .Select(OrganizationProjections.ToDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetOrganizationsResult(organizations);
    }
}
