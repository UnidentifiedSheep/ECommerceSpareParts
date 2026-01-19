using Application.Common.Interfaces;
using Core.Models;
using Main.Abstractions.Dtos.Roles;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Roles.GetRoles;

public record GetRolesQuery(string? SearchTerm, PaginationModel Pagination) : IQuery<GetRolesResult>;

public record GetRolesResult(IEnumerable<RoleDto> Roles);

public class GetRolesHandler(IRoleRepository roleRepository) : IQueryHandler<GetRolesQuery, GetRolesResult>
{
    public async Task<GetRolesResult> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var limit = request.Pagination.Size;
        var roles = await roleRepository.SearchRoles(request.SearchTerm, page, limit, false, cancellationToken);
        return new GetRolesResult(roles.Adapt<List<RoleDto>>());
    }
}