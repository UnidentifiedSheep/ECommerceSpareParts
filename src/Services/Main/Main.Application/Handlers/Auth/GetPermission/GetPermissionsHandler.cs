using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Auth;
using Main.Application.Projections;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth.GetPermission;

[Diagnostics(maxExecutionTimeMs: 80)]
public record GetPermissionsQuery(Pagination Pagination) : IQuery<GetPermissionsResult>;

public record GetPermissionsResult(IReadOnlyList<PermissionDto> Permissions);

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