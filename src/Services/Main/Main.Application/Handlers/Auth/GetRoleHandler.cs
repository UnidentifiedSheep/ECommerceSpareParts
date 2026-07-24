using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth.GetPermissions;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth;

public record GetRoleQuery(string Name) : IQuery<GetRoleResult>;

public record GetRoleResult(RoleDto Role, IReadOnlyList<PermissionDto> Permissions);

public class GetRoleHandler(
    IReadRepository<Role, string> readRepository,
    ISender sender,
    IProjectionProvider<Role, RoleDto> projection
) : IQueryHandler<GetRoleQuery, GetRoleResult>
{
    public async Task<GetRoleResult> Handle(
        GetRoleQuery request,
        CancellationToken cancellationToken)
    {
        var roleToDto = projection.Projection;

        var permissions = (await sender.Send(new GetPermissionsQuery(), cancellationToken))
            .Permissions;

        var roleWithPermissions = await readRepository.Query
                                      .Where(x => x.Name == request.Name)
                                      .AsExpandable()
                                      .Select(x => new
                                      {
                                          Role = roleToDto.Invoke(x),
                                          Permissions = x.RolePermissions.Select(z => z.PermissionName)
                                      })
                                      .FirstOrDefaultAsync(cancellationToken)
                                  ?? throw new RoleNotFoundException(request.Name);

        return new GetRoleResult(
            roleWithPermissions.Role,
            permissions.Where(x => roleWithPermissions.Permissions.Contains(x.SystemName)).ToList());
    }
}
