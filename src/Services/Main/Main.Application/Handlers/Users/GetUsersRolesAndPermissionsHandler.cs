using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Application.Common.Interfaces.Repositories;
using Main.Application.Extensions.QueryExtensions;
using Main.Entities.User;
using Main.Entities.Exceptions;
using Main.Entities.User;

namespace Main.Application.Handlers.Users;

public sealed record GetUsersRolesAndPermissionsQuery : IQuery<GetUsersRolesAndPermissionsResult>
{
	public GetUsersRolesAndPermissionsQuery(Guid id) : this([id]) { }

	public GetUsersRolesAndPermissionsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetUsersRolesAndPermissionsResult(
	IReadOnlyDictionary<Guid, UserRolesAndPermissions> Users);

public sealed class GetUsersRolesAndPermissionsHandler(IReadRepository<User, Guid> repository)
	: IQueryHandler<GetUsersRolesAndPermissionsQuery, GetUsersRolesAndPermissionsResult>
{
	public async Task<GetUsersRolesAndPermissionsResult> Handle(
		GetUsersRolesAndPermissionsQuery request,
		CancellationToken cancellationToken)
	{
		var users = await repository
			.GetUsersRolesAndPermissionsAsync(request.Ids, cancellationToken);
		if (request.Ids.Count == 1 && !users.ContainsKey(request.Ids[0]))
			throw new UserNotFoundException(request.Ids[0]);

		return new GetUsersRolesAndPermissionsResult(users);
	}
}
