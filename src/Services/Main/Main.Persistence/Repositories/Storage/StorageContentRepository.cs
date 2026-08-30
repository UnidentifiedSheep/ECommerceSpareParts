using System.Data;
using Abstractions.Models;
using Dapper;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models.Storage;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Entities.Storage;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Storage;

public class StorageContentRepository(DContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<DContext, StorageContent, int>(context, extensions), IStorageContentRepository
{
	public async Task<IReadOnlyList<StorageContentPageItem>> GetByProductsAsync(
		IReadOnlyCollection<int> productIds,
		Pagination pagination,
		string? storageCode,
		bool showZeroCount,
		CancellationToken cancellationToken = default)
	{
		if (productIds.Count == 0)
			return [];

		var storageFilter = string.IsNullOrWhiteSpace(storageCode)
			? string.Empty
			: "AND sc.storage_name = @StorageCode";
		var countFilter = showZeroCount ? string.Empty : "AND sc.count > 0";
		var sql = $"""
					SELECT ranked.product_id AS "ProductId",
					       ranked.id AS "StorageContentId"
					FROM (
					    SELECT sc.product_id,
					           sc.id,
					           row_number() OVER (
					               PARTITION BY sc.product_id
					               ORDER BY sc.id) AS row_number
					    FROM public.storage_content AS sc
					    WHERE sc.product_id = ANY(@ProductIds)
					      {storageFilter}
					      {countFilter}
					) AS ranked
					WHERE ranked.row_number > @Offset
					  AND ranked.row_number <= @Limit
					ORDER BY ranked.product_id, ranked.row_number
					""";

		var connection = Context.Database.GetDbConnection();
		var shouldClose = connection.State != ConnectionState.Open;
		if (shouldClose)
			await connection.OpenAsync(cancellationToken);

		try
		{
			var command = new CommandDefinition(
				sql,
				new
				{
					ProductIds = productIds.ToArray(),
					StorageCode = storageCode,
					Offset = pagination.Page * pagination.Size,
					Limit = (pagination.Page + 1) * pagination.Size
				},
				Context.Database.CurrentTransaction?.GetDbTransaction(),
				cancellationToken: cancellationToken);

			return (await connection.QueryAsync<StorageContentPageItem>(command)).AsList();
		}
		finally
		{
			if (shouldClose)
				await connection.CloseAsync();
		}
	}

	public IAsyncEnumerable<StorageContent> GetStorageContentsForUpdateAsync(
		int? productId,
		string? storageCode,
		IEnumerable<int>? exceptProductIds = null,
		IEnumerable<string>? exceptStorages = null,
		int countGreaterThen = 0,
		StorageContentExtractPolicyBase? policy = null)
	{
		return BuildStorageContentsForUpdateQuery(
				productId,
				storageCode,
				exceptProductIds,
				exceptStorages,
				countGreaterThen,
				policy)
			.AsAsyncEnumerable();
	}

	public async Task<Dictionary<int, int>> GetStorageContentCounts(
		string storageCode,
		IEnumerable<int> productIds,
		bool takeFromOtherStorages,
		CancellationToken cancellationToken = default)
	{
		return await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.Count > 0 &&
				productIds.Contains(x.ProductId) && (takeFromOtherStorages || x.StorageCode == storageCode))
			.GroupBy(x => x.ProductId)
			.Select(g => new
			{
				ProductId = g.Key, TotalCount = g.Sum(x => x.Count)
			})
			.ToDictionaryAsync(
				x => x.ProductId,
				x => x.TotalCount,
				cancellationToken);
	}

	private IQueryable<StorageContent> BuildStorageContentsForUpdateQuery(
		int? productId,
		string? storageCode,
		IEnumerable<int>? exceptProductIds = null,
		IEnumerable<string>? exceptStorages = null,
		int countGreaterThen = 0,
		StorageContentExtractPolicyBase? policy = null)
	{
		var exceptProducts = exceptProductIds?.ToList();
		var exceptStorageCodes = exceptStorages?.ToList();
		var query = Context.StorageContents.Where(x => x.Count > countGreaterThen);

		if (productId != null)
			query = query.Where(x => x.ProductId == productId);

		if (exceptProducts is { Count: > 0 })
			query = query.Where(x => !exceptProducts.Contains(x.ProductId));

		if (storageCode != null)
			query = query.Where(x => x.StorageCode == storageCode);

		if (exceptStorageCodes is { Count: > 0 })
			query = query.Where(x => !exceptStorageCodes.Contains(x.StorageCode));

		query = QueryableExtensions.ForUpdate(query);

		return policy != null ? policy.Apply(query) : query.OrderBy(x => x.PurchaseDatetime);
	}
}
