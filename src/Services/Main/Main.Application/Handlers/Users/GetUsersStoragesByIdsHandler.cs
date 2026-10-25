using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users;

public sealed record GetUsersStoragesByIdsQuery : IQuery<GetUsersStoragesByIdsResult>
{
	public GetUsersStoragesByIdsQuery(Guid id) : this([id]) { }

	public GetUsersStoragesByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetUsersStoragesByIdsResult(
	IReadOnlyDictionary<Guid, IReadOnlyList<StorageDto>> Storages);

public sealed class GetUsersStoragesByIdsHandler(
	IReadRepository<StorageOwner, (string, Guid)> repository,
	IProjectionProvider<Storage, StorageDto> projection)
	: IQueryHandler<GetUsersStoragesByIdsQuery, GetUsersStoragesByIdsResult>
{
	public async Task<GetUsersStoragesByIdsResult> Handle(
		GetUsersStoragesByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetUsersStoragesByIdsResult(new Dictionary<Guid, IReadOnlyList<StorageDto>>());

		var rows = await repository.Query
			.AsExpandable()
			.Where(x => request.Ids.Contains(x.UserId))
			.OrderBy(x => x.Storage.Code)
			.Select(x => new
			{
				x.UserId,
				Dto = projection.Projection.Invoke(x.Storage)
			})
			.ToListAsync(cancellationToken);

		return new GetUsersStoragesByIdsResult(rows
			.GroupBy(x => x.UserId, x => x.Dto)
			.ToDictionary(x => x.Key, IReadOnlyList<StorageDto> (x) => x.ToArray()));
	}
}
