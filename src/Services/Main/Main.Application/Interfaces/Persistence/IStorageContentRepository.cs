using Abstractions.Models;
using Application.Common.Interfaces.Repositories;
using Main.Application.Models.Storage;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
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

	IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(
		int? productId,
		string? storageCode,
		IEnumerable<int>? exceptProductIds = null,
		IEnumerable<string>? exceptStorages = null,
		int countGreaterThen = 0,
		StorageContentExtractPolicyBase? policy = null);

	Task<Dictionary<int, int>> GetStorageContentCounts(
		string storageCode,
		IEnumerable<int> productIds,
		bool takeFromOtherStorages,
		CancellationToken cancellationToken = default);
}
