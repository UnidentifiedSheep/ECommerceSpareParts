using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Amw.Permissions;
using Main.Application.Handlers.Currencies.Projections;
using Main.Entities.Auth;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Permissions.GetPermission;

public record GetPermissionsQuery(PaginationModel Pagination) : IQuery<GetPermissionsResult>;

public record GetPermissionsResult(IEnumerable<PermissionDto> Permissions);

public class GetPermissionsHandler(IReadRepository<Permission, string> repository)
    : IQueryHandler<GetPermissionsQuery, GetPermissionsResult>
{
    public async Task<GetPermissionsResult> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await repository.Query
            .OrderBy(x => x.Name)
            .Select(AuthProjections.ToPermissionDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetPermissionsResult(permissions);
    }
}