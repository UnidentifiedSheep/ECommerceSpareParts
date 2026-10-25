using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users;

public sealed record GetUsersPhonesByIdsQuery : IQuery<GetUsersPhonesByIdsResult>
{
	public GetUsersPhonesByIdsQuery(Guid id) : this([id]) { }

	public GetUsersPhonesByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetUsersPhonesByIdsResult(IReadOnlyDictionary<Guid, IReadOnlyList<UserPhoneDto>> Phones);


public class GetUsersPhonesByIdsHandler(
	IReadRepository<UserPhone, string> repository,
	IProjectionProvider<UserPhone, UserPhoneDto> projection
	) : IQueryHandler<GetUsersPhonesByIdsQuery, GetUsersPhonesByIdsResult>
{
	public async Task<GetUsersPhonesByIdsResult> Handle(
		GetUsersPhonesByIdsQuery request,
		CancellationToken cancellationToken)
	{
		var phones = (await repository.Query
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
				IReadOnlyList<UserPhoneDto> (x) => x
					.OrderByDescending(z => z.IsPrimary).ToList());

		return new GetUsersPhonesByIdsResult(phones);
	}
}
