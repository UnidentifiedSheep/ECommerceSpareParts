using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users;

public sealed record GetUsersEmailsByIdsQuery : IQuery<GetUsersEmailsByIdsResult>
{
	public GetUsersEmailsByIdsQuery(Guid id) : this([id]) { }

	public GetUsersEmailsByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetUsersEmailsByIdsResult(IReadOnlyDictionary<Guid, IReadOnlyList<UserEmailDto>> Emails);

public sealed class GetUsersEmailsByIdsHandler(
	IReadRepository<UserEmail, string> repository,
	IProjectionProvider<UserEmail, UserEmailDto> projection)
	: IQueryHandler<GetUsersEmailsByIdsQuery, GetUsersEmailsByIdsResult>
{
	public async Task<GetUsersEmailsByIdsResult> Handle(
		GetUsersEmailsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (request.Ids.Count == 0)
			return new GetUsersEmailsByIdsResult(new Dictionary<Guid, IReadOnlyList<UserEmailDto>>());

		var emails = (await repository
				.Query
				.AsExpandable()
				.Where(x => request.Ids.Contains(x.UserId))
				.Select(x => new
				{
					x.UserId,
					Dto = projection.Projection.Invoke(x)
				})
				.ToListAsync(cancellationToken))
			.GroupBy(x => x.UserId, x => x.Dto)
			.ToDictionary(
				x => x.Key,
				IReadOnlyList<UserEmailDto> (x) => x.OrderByDescending(z => z.IsPrimary).ToList());

		if (request.Ids.Count == 1 && !emails.ContainsKey(request.Ids[0]))
			throw new UserNotFoundException(request.Ids[0]);

		return new GetUsersEmailsByIdsResult(emails);
	}
}
