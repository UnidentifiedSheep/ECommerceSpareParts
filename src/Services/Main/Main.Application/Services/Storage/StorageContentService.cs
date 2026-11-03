using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using FluentValidation;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Storage;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
using Main.Entities.Storage;
using Main.Enums;
using NamedObject.Core.Interfaces;

namespace Main.Application.Services.Storage;

public sealed class StorageContentService(
	IRepository<StorageContent, int> repository,
	IReadRepository<StorageContentLock, Guid> lockRepository,
	ISettingsService settingsService,
	INamedObjectRegistry<StorageContentExtractPolicyBase> policyRegistry,
	IValidator<StorageContentWithdrawal> validator,
	IUnitOfWork unitOfWork,
	TimeProvider timeProvider) : IStorageContentService
{
	public async Task<IReadOnlyList<StorageLot>> SubtractAsync(
		IReadOnlyCollection<ISubtractStorageContentItem> items,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
	{
		await validator.ValidateAndThrowAsync(new StorageContentWithdrawal(items, movementType), cancellationToken);
		EnsureTransaction();
		var byContent = new Dictionary<int, int>();
		var byProduct = new Dictionary<(int ProductId, string? StorageCode), int>();
		foreach (var item in items)
			switch (item)
			{
				case SubtractStorageContentItem content:
					byContent[content.StorageContentId] = checked(byContent.GetValueOrDefault(content.StorageContentId) + content.Count);
					break;
				case SubtractProductFromStorageItem product:
					var key = (product.ProductId, product.StorageCode);
					byProduct[key] = checked(byProduct.GetValueOrDefault(key) + product.Count);
					break;
				default:
					throw new InvalidOperationException($"Unsupported subtract item type {item.GetType().Name}.");
			}

		var preferred = byContent.Count == 0
			? []
			: await repository.ListAsync(Criteria<StorageContent>.New()
				.Where(content => byContent.Keys.Contains(content.Id)).Build(), cancellationToken);
		byContent.Keys.EnsureAllExists(preferred.Select(content => content.Id),
			missing => new StorageContentNotFoundException(missing[0]));

		var selections = byProduct.Keys
			.Select(key => new StorageContentSelection(key.ProductId, key.StorageCode))
			.Concat(preferred.Select(content => new StorageContentSelection(content.ProductId, content.StorageCode)))
			.Distinct()
			.ToArray();

		var selectionPredicate = PredicateBuilder.New<StorageContent>(false);
		foreach (var selection in selections)
			selectionPredicate = selectionPredicate.Or(content => content.ProductId == selection.ProductId &&
				(selection.StorageCode == null || content.StorageCode == selection.StorageCode));

		var preferredIds = byContent.Keys.ToArray();
		var contents = await repository.ListAsync(Criteria<StorageContent>.New()
			.Where(selectionPredicate)
			.Where(content => content.Count > 0 || preferredIds.Contains(content.Id))
			.OrderByAsc(content => content.Id).Track().ForUpdate().Build(), cancellationToken);
		var contentsById = contents.ToDictionary(content => content.Id);
		byContent.Keys.EnsureAllExists(contentsById.Keys, missing => new StorageContentNotFoundException(missing[0]));
		// A separate statement after acquiring row locks sees reservations committed while we waited.
		var locked = await GetLockedQuantitiesAsync(contentsById.Keys.ToArray(),
			timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
		var setting = (await settingsService.GetOrDefault<StorageContentSetting>(cancellationToken)).Data;
		var policy = policyRegistry.GetBySystemName(setting.StorageContentExtractionPolicy);
		var affected = new List<StorageLot>();

		foreach (var (id, count) in byContent)
		{
			var content = contentsById[id];
			var remaining = count;
			Subtract(content, ref remaining);
			Take(content.ProductId, content.StorageCode, ref remaining, id);
			EnsureEnough(content.ProductId, count, remaining);
		}
		foreach (var (key, count) in byProduct)
		{
			var remaining = count;
			Take(key.ProductId, key.StorageCode, ref remaining);
			EnsureEnough(key.ProductId, count, remaining);
		}

		return affected;

		void Take(int productId, string? storageCode, ref int remaining, int? exceptId = null)
		{
			var candidates = contents.Where(content => content.ProductId == productId &&
				(storageCode is null || content.StorageCode == storageCode) && content.Id != exceptId).AsQueryable();
			foreach (var content in policy.Apply(candidates).ThenBy(content => content.Id))
			{
				if (remaining == 0) break;
				Subtract(content, ref remaining);
			}
		}

		void Subtract(StorageContent content, ref int remaining)
		{
			var available = Math.Max(0L, content.Count - locked.GetValueOrDefault(content.Id));
			var count = (int)Math.Min(available, remaining);
			if (count == 0) return;
			content.IncreaseCount(-count, movementType);
			affected.Add(new StorageLot(content.Id, content.ProductId, content.CurrencyId,
				content.BuyPrice, count, content.PurchaseDatetime));
			remaining -= count;
		}
	}

	public async Task EnsureCanSetQuantitiesAsync(
		IReadOnlyCollection<StorageContentQuantityChange> changes,
		CancellationToken cancellationToken = default)
	{
		if (changes.Count == 0) return;
		EnsureTransaction();
		var locked = await GetLockedQuantitiesAsync(changes.Select(change => change.Content.Id).ToArray(),
			timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
		foreach (var change in changes)
			if (change.Quantity < locked.GetValueOrDefault(change.Content.Id))
				throw new NotEnoughCountOnStorageException(change.Content.ProductId,
					(int)Math.Max(0L, change.Content.Count - locked.GetValueOrDefault(change.Content.Id)),
					change.Content.Count - change.Quantity);
	}

	private Task<Dictionary<int, long>> GetLockedQuantitiesAsync(
		IReadOnlyCollection<int> contentIds, DateTime utcNow, CancellationToken cancellationToken) =>
		lockRepository.Query
			.Where(item => contentIds.Contains(item.StorageContentId) && item.State == StorageContentLockState.Active &&
				(item.ExpiresAt == null || item.ExpiresAt > utcNow))
			.GroupBy(item => item.StorageContentId)
			.Select(group => new { Id = group.Key, Quantity = group.Sum(item => (long)item.Quantity) })
			.ToDictionaryAsync(item => item.Id, item => item.Quantity, cancellationToken);

	private void EnsureTransaction()
	{
		if (!unitOfWork.HasActiveTransaction)
			throw new InvalidOperationException("Storage content changes require an active transaction.");
	}

	private static void EnsureEnough(int productId, int requested, int remaining)
	{
		if (remaining > 0)
			throw new NotEnoughCountOnStorageException(productId, requested - remaining, requested);
	}
}
