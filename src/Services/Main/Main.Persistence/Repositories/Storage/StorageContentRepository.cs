using System.Data;
using System.Linq.Expressions;
using Abstractions.Models;
using Application.Common.Models;
using Dapper;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models.Storage;
using Main.Enums;
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

	public Task<Dictionary<int, StorageContent>> GetByIdsForUpdateAsync(
		IReadOnlyCollection<int> contentIds, CancellationToken cancellationToken = default)
	{
		if (contentIds.Count == 0) return Task.FromResult(new Dictionary<int, StorageContent>());
		var query = Context.StorageContents.AsTracking().Where(content => contentIds.Contains(content.Id))
			.OrderBy(content => content.Id);
		return QueryableExtensions.ForUpdate(query).ToDictionaryAsync(content => content.Id, cancellationToken);
	}

	public async Task<IReadOnlyList<StorageContent>> GetForUpdateAsync(
		IReadOnlyCollection<StorageContentSelection> selections,
		IReadOnlyCollection<int> preferredContentIds,
		CancellationToken cancellationToken = default)
	{
		if (selections.Count == 0) return [];
		var includedIds = preferredContentIds.Concat(Context.ChangeTracker.Entries<StorageContent>()
			.Where(entry => entry.State == EntityState.Modified && entry.Entity.Count > 0)
			.Select(entry => entry.Entity.Id)).Distinct().ToArray();
		var query = Context.StorageContents.AsTracking().Where(BuildSelectionPredicate(selections))
			.Where(content => content.Count > 0 || includedIds.Contains(content.Id))
			.OrderBy(content => content.Id);
		return await QueryableExtensions.ForUpdate(query).ToListAsync(cancellationToken);
	}

	public async Task<Dictionary<int, long>> GetLockedQuantitiesAsync(
		IReadOnlyCollection<int> contentIds, DateTime utcNow, CancellationToken cancellationToken = default)
	{
		if (contentIds.Count == 0) return [];
		var quantities = await Context.StorageContentLocks.AsNoTracking()
			.Where(item => contentIds.Contains(item.StorageContentId) && item.State == StorageContentLockState.Active &&
				(item.ExpiresAt == null || item.ExpiresAt > utcNow))
			.GroupBy(item => item.StorageContentId)
			.Select(group => new { Id = group.Key, Quantity = group.Sum(item => (long)item.Quantity) })
			.ToDictionaryAsync(item => item.Id, item => item.Quantity, cancellationToken);

		// Include reservations changed in this transaction before their next SaveChanges.
		var ids = contentIds.ToHashSet();
		foreach (var entry in Context.ChangeTracker.Entries<StorageContentLock>())
		{
			if (!ids.Contains(entry.Entity.StorageContentId) || entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
				continue;
			long original = 0;
			if (entry.State != EntityState.Added)
			{
				var state = (StorageContentLockState)entry.OriginalValues[nameof(StorageContentLock.State)]!;
				var expiresAt = (DateTime?)entry.OriginalValues[nameof(StorageContentLock.ExpiresAt)];
				if (state == StorageContentLockState.Active && (expiresAt is null || expiresAt > utcNow))
					original = (int)entry.OriginalValues[nameof(StorageContentLock.Quantity)]!;
			}
			var current = entry.State != EntityState.Deleted && entry.Entity.IsActiveAt(utcNow) ? (long)entry.Entity.Quantity : 0L;
			var id = entry.Entity.StorageContentId;
			quantities[id] = checked(quantities.GetValueOrDefault(id) + current - original);
		}
		return quantities;
	}

	public async Task<Dictionary<(int ProductId, string StorageCode), int>> GetAvailableCountsAsync(
		IReadOnlyCollection<StorageContentSelection> selections, DateTime utcNow, CancellationToken cancellationToken = default)
	{
		if (selections.Count == 0) return [];
		var rows = await Context.StorageContents.AsNoTracking()
			.Where(BuildSelectionPredicate(selections))
			.Where(content => content.Count > 0)
			.Select(content => new
			{
				content.ProductId, content.StorageCode,
				Available = Math.Max(0L, (long)content.Count - (Context.StorageContentLocks
					.Where(item => item.StorageContentId == content.Id && item.State == StorageContentLockState.Active &&
						(item.ExpiresAt == null || item.ExpiresAt > utcNow))
					.Sum(item => (long?)item.Quantity) ?? 0L))
			})
			.GroupBy(content => new { content.ProductId, content.StorageCode })
			.Select(group => new { group.Key.ProductId, group.Key.StorageCode, Quantity = group.Sum(content => content.Available) })
			.ToListAsync(cancellationToken);
		return rows.ToDictionary(item => (item.ProductId, item.StorageCode), item => checked((int)item.Quantity));
	}

	private static Expression<Func<StorageContent, bool>> BuildSelectionPredicate(
		IReadOnlyCollection<StorageContentSelection> selections)
	{
		var parameter = Expression.Parameter(typeof(StorageContent), "content");
		Expression body = Expression.Constant(false);
		foreach (var selection in selections.Distinct())
		{
			Expression match = Expression.Equal(Expression.Property(parameter, nameof(StorageContent.ProductId)),
				Expression.Constant(selection.ProductId));
			if (selection.StorageCode is not null)
				match = Expression.AndAlso(match, Expression.Equal(Expression.Property(parameter, nameof(StorageContent.StorageCode)),
					Expression.Constant(selection.StorageCode)));
			body = Expression.OrElse(body, match);
		}
		return Expression.Lambda<Func<StorageContent, bool>>(body, parameter);
	}
}
