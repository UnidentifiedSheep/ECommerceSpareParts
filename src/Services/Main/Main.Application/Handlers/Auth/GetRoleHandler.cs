using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Localization.Abstractions.Interfaces;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth.GetPermissions;
using Main.Application.Projections;
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
    IScopedStringLocalizer localizer
) : IQueryHandler<GetRoleQuery, GetRoleResult>
{
    public async Task<GetRoleResult> Handle(
        GetRoleQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = (await sender.Send(new GetPermissionsQuery(), cancellationToken))
            .Permissions;

        var roleWithPermissions = await readRepository.Query
                                      .Where(x => x.Name == request.Name)
                                      .AsExpandable()
                                      .Select(x => new
                                      {
                                          Role = AuthProjections.ToRoleDto(localizer).Invoke(x),
                                          Permissions = x.RolePermissions.Select(z => z.PermissionName)
                                      })
                                      .FirstOrDefaultAsync(cancellationToken)
                                  ?? throw new RoleNotFoundException(request.Name);

        return new GetRoleResult(
            roleWithPermissions.Role,
            permissions.Where(x => roleWithPermissions.Permissions.Contains(x.SystemName)).ToList());
    }
}