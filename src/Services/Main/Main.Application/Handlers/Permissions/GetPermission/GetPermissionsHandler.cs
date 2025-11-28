using Application.Common.Interfaces;
using Core.Models;
using Main.Core.Dtos.Amw.Permissions;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Permissions.GetPermission;

public record GetPermissionsQuery(PaginationModel Pagination) : IQuery<GetPermissionsResult>;
public record GetPermissionsResult(IEnumerable<PermissionDto> Permissions);

public class GetPermissionsHandler(IPermissionRepository permissionRepository) : IQueryHandler<GetPermissionsQuery, GetPermissionsResult>
{
    public async Task<GetPermissionsResult> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        int page = request.Pagination.Page;
        int limit = request.Pagination.Size;
        var permissions = await permissionRepository
            .GetPermissionsAsync(page, limit, false, cancellationToken);
        return new GetPermissionsResult(permissions.Adapt<List<PermissionDto>>());
    }
}