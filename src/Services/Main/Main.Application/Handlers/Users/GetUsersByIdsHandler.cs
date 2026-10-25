using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users;

public sealed record GetUsersByIdsQuery : IQuery<GetUsersByIdsResult>
{
	public IReadOnlyList<Guid> Ids { get; }

	public GetUsersByIdsQuery(Guid id) : this([id]) { }

	public GetUsersByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}
}

public sealed record GetUsersByIdsResult(IReadOnlyDictionary<Guid, UserDto> Users);

public sealed class GetUsersByIdsHandler(
	IReadRepository<User, Guid> repository,
	IProjectionProvider<User, UserDto> projection)
	: IQueryHandler<GetUsersByIdsQuery, GetUsersByIdsResult>
{
	public async Task<GetUsersByIdsResult> Handle(
		GetUsersByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetUsersByIdsResult(new Dictionary<Guid, UserDto>());

		var users = await repository
			.Query
			.Where(x => request.Ids.Contains(x.Id))
			.Project(projection)
			.ToDictionaryAsync(x => x.Id, cancellationToken);

		if (request.Ids.Count == 1 && !users.ContainsKey(request.Ids[0]))
			throw new UserNotFoundException(request.Ids[0]);

		return new GetUsersByIdsResult(users);
	}
}
