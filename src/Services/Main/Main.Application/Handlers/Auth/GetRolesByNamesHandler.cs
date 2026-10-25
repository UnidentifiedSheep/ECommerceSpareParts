using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Auth;
using Main.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth;

public sealed record GetRolesByNamesQuery : IQuery<GetRolesByNamesResult>
{
	public GetRolesByNamesQuery(IEnumerable<string> names)
	{
		Names = names.Select(RoleNames.Normalize).Distinct().ToArray();
	}

	public IReadOnlyList<string> Names { get; }
}

public sealed record GetRolesByNamesResult(IReadOnlyDictionary<string, RoleDto> Roles);

public sealed class GetRolesByNamesHandler(
	IReadRepository<Role, string> repository,
	IProjectionProvider<Role, RoleDto> projection) : IQueryHandler<GetRolesByNamesQuery, GetRolesByNamesResult>
{
	public async Task<GetRolesByNamesResult> Handle(GetRolesByNamesQuery request, CancellationToken cancellationToken)
	{
		if (request.Names.Count == 0)
			return new GetRolesByNamesResult(new Dictionary<string, RoleDto>());

		var roles = await repository.Query
			.Where(role => request.Names.Contains(role.Name))
			.Project(projection)
			.ToDictionaryAsync(role => role.SystemName, cancellationToken);
		return new GetRolesByNamesResult(roles);
	}
}
