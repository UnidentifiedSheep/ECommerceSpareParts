using Abstractions.Models;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Models.Storage;
using Main.Entities.Storage;

namespace Main.Application.Interfaces.Persistence;

public interface IStorageContentRepository : IRepository<StorageContent, int>
{
	Task<IReadOnlyList<StorageContentPageItem>> GetByProductsAsync(
		IReadOnlyCollection<int> productIds,
		Pagination pagination,
		string? storageCode,
		bool showZeroCount,
		CancellationToken cancellationToken = default);

	Task<Dictionary<int, StorageContent>> GetByIdsForUpdateAsync(
		IReadOnlyCollection<int> contentIds,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<StorageContent>> GetForUpdateAsync(
		IReadOnlyCollection<StorageContentSelection> selections,
		IReadOnlyCollection<int> preferredContentIds,
		CancellationToken cancellationToken = default);

	Task<Dictionary<int, long>> GetLockedQuantitiesAsync(
		IReadOnlyCollection<int> contentIds,
		DateTime utcNow,
		CancellationToken cancellationToken = default);

	Task<Dictionary<(int ProductId, string StorageCode), int>> GetAvailableCountsAsync(
		IReadOnlyCollection<StorageContentSelection> selections,
		DateTime utcNow,
		CancellationToken cancellationToken = default);
}
