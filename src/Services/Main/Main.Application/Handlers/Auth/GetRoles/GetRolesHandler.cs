using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Projections;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth.GetRoles;

public record GetRolesQuery(string? SearchTerm, Pagination Pagination) : IQuery<GetRolesResult>;

public record GetRolesResult(IEnumerable<RoleDto> Roles);

public class GetRolesHandler(IReadRepository<Role, string> repository) : IQueryHandler<GetRolesQuery, GetRolesResult>
{
    public async Task<GetRolesResult> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var trimmed = request.SearchTerm?.Trim();

        var query = repository.Query;

        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name.NormalizedValue, $"%{trimmed}%"))
                .Select(x => new
                    { Role = x, Rank = EF.Functions.TrigramsSimilarity(x.Name.NormalizedValue, $"%{trimmed}%") })
                .OrderByDescending(x => x.Rank)
                .Select(x => x.Role);
        }
        else
            query = query.OrderBy(x => x.Name.NormalizedValue);
        
        var roles = await query
            .AsExpandable()
            .Select(AuthProjections.ToRoleDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetRolesResult(roles);
    }
}