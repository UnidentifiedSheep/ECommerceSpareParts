using Application.Common.Interfaces.Repositories;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Abstractions.Events;
using Main.Modules.Storage.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Storage.Application.Services.Storage;

public sealed class StorageContentSnapshotReader(IReadRepository<StorageContent, int> repository)
	: IStorageContentSnapshotReader
{
	public async Task<IReadOnlyList<StorageContentSnapshot>> GetAfterAsync(
		int afterStorageContentId,
		int take,
		CancellationToken cancellationToken = default)
		=> await repository
			.Query
			.Where(x => x.Id > afterStorageContentId)
			.OrderBy(x => x.Id)
			.Take(take)
			.Select(x => new StorageContentSnapshot(
				x.Id,
				x.ProductId,
				x.StorageCode,
				x.CurrencyId,
				x.BuyPrice,
				x.Count))
			.ToListAsync(cancellationToken);
}
