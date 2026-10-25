using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth.GetPermissions;
using Main.Entities.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth;

public sealed record GetRolesPermissionsByNamesQuery : IQuery<GetRolesPermissionsByNamesResult>
{
	public GetRolesPermissionsByNamesQuery(IEnumerable<string> names)
	{
		Names = names.Select(RoleNames.Normalize).Distinct().ToArray();
	}

	public IReadOnlyList<string> Names { get; }
}

public sealed record GetRolesPermissionsByNamesResult(
	IReadOnlyDictionary<string, IReadOnlyList<PermissionDto>> Permissions);

public sealed class GetRolesPermissionsByNamesHandler(IReadRepository<Role, string> repository, ISender sender)
	: IQueryHandler<GetRolesPermissionsByNamesQuery, GetRolesPermissionsByNamesResult>
{
	public async Task<GetRolesPermissionsByNamesResult> Handle(
		GetRolesPermissionsByNamesQuery request, CancellationToken cancellationToken)
	{
		if (request.Names.Count == 0)
			return new GetRolesPermissionsByNamesResult(new Dictionary<string, IReadOnlyList<PermissionDto>>());

		var roles = await repository.Query
			.Where(role => request.Names.Contains(role.Name))
			.Select(role => new
			{
				role.Name,
				Permissions = role.RolePermissions.Select(permission => permission.PermissionName).ToArray()
			})
			.ToListAsync(cancellationToken);

		if (roles.Count == 0)
			return new GetRolesPermissionsByNamesResult(new Dictionary<string, IReadOnlyList<PermissionDto>>());

		var catalog = (await sender.Send(new GetPermissionsQuery(), cancellationToken)).Permissions;
		var permissions = roles.ToDictionary(
			role => role.Name,
			role => (IReadOnlyList<PermissionDto>)catalog
				.Where(permission => role.Permissions.Contains(permission.SystemName))
				.ToArray());
		return new GetRolesPermissionsByNamesResult(permissions);
	}
}
