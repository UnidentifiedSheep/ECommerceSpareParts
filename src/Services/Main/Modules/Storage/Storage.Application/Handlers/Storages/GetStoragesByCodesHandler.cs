using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Modules.Storage.Application.Dtos.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Storage.Application.Handlers.Storages;

public record GetStoragesByCodesQuery : IQuery<GetStoragesByCodesResult>
{

	public GetStoragesByCodesQuery(IEnumerable<string> codes) => StorageCodes = codes.Distinct().ToList();

	public GetStoragesByCodesQuery(string code) : this([code])
	{
	}

	public IReadOnlyList<string> StorageCodes { get; }
}

public record GetStoragesByCodesResult(IReadOnlyList<StorageDto> Storages);

public class GetStoragesByCodesHandler(
	IReadRepository<Entities.Storage.Storage.Storage, string> repository,
	IProjectionProvider<Entities.Storage.Storage.Storage, StorageDto> projection)
	: IQueryHandler<GetStoragesByCodesQuery, GetStoragesByCodesResult>
{
	public async Task<GetStoragesByCodesResult> Handle(
		GetStoragesByCodesQuery request,
		CancellationToken cancellationToken)
		=> new(
			await repository
				.Query
				.Where(x => request.StorageCodes.Contains(x.Code))
				.Project(projection)
				.ToListAsync(cancellationToken));
}
