using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using FluentValidation;
using LinqKit;
using Main.Application.Extensions;
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
	ISettingsService settingsService,
	INamedObjectRegistry<StorageContentExtractPolicyBase> policyRegistry,
	IValidator<IReadOnlyCollection<SubtractStorageContentItem>> contentValidator,
	IValidator<IReadOnlyCollection<SubtractProductFromStorageItem>> productValidator,
	IUnitOfWork unitOfWork,
	TimeProvider timeProvider) : IStorageContentService
{
	public async Task<IReadOnlyList<StorageLot>> SubtractByProductsAsync(
		IReadOnlyCollection<SubtractProductFromStorageItem> items,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
	{
		await productValidator.ValidateAndThrowAsync(items, cancellationToken);
		EnsureTransaction();
		var requests = items.GroupBy(item => (item.ProductId, item.StorageCode))
			.Select(group => new
			{
				group.Key,
				Count = checked((int)group.Sum(item => (long)item.Count))
			})
			.ToArray();

		var contents = await LoadByProductsForUpdateAsync(
			requests.Select(request => request.Key),
			[],
			cancellationToken);

		var policy = await GetPolicyAsync(cancellationToken);
		var utcNow = timeProvider.GetUtcNow().UtcDateTime;
		var affected = new List<StorageLot>();

		foreach (var request in requests)
		{
			var candidates = contents
				.Where(content => content.ProductId == request.Key.ProductId &&
					content.StorageCode == request.Key.StorageCode)
				.AsQueryable();

			var remaining = SubtractFrom(
				policy.Apply(candidates)
					.ThenBy(content => content.Id),
				request.Count,
				movementType,
				utcNow,
				affected);

			EnsureEnough(request.Key.ProductId, request.Count, remaining);
		}

		return affected;
	}

	public async Task<IReadOnlyList<StorageLot>> SubtractByContentsAsync(
		IReadOnlyCollection<SubtractStorageContentItem> items,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
	{
		await contentValidator.ValidateAndThrowAsync(items, cancellationToken);
		EnsureTransaction();
		var requests = items.GroupBy(item => item.StorageContentId)
			.ToDictionary(
				group => group.Key,
				group => checked((int)group.Sum(item => (long)item.Count)));

		var preferredIds = requests.Keys.ToArray();
		var preferred = await repository.GetWithActiveLocksForUpdateAsync(
			preferredIds, cancellationToken: cancellationToken);

		var utcNow = timeProvider.GetUtcNow().UtcDateTime;
		var affected = new List<StorageLot>();
		var outstanding = new List<(StorageContent Content, int Requested, int Remaining)>();
		foreach (var (id, count) in requests)
		{
			var content = preferred[id];
			var remaining = SubtractFrom([content], count, movementType, utcNow, affected);
			if (remaining > 0)
				outstanding.Add((content, count, remaining));
		}

		if (outstanding.Count == 0) return affected;

		var additional = await LoadByProductsForUpdateAsync(
			outstanding.Select(item => (item.Content.ProductId, item.Content.StorageCode)),
			preferredIds,
			cancellationToken);
		var policy = await GetPolicyAsync(cancellationToken);
		foreach (var (content, requested, remaining) in outstanding)
		{
			var candidates = preferred.Values.Concat(additional).Where(candidate =>
				candidate.ProductId == content.ProductId && candidate.StorageCode == content.StorageCode &&
				candidate.Id != content.Id).AsQueryable();
			var shortage = SubtractFrom(policy.Apply(candidates).ThenBy(candidate => candidate.Id),
				remaining, movementType, utcNow, affected);
			EnsureEnough(content.ProductId, requested, shortage);
		}

		return affected;
	}

	private Task<List<StorageContent>> LoadByProductsForUpdateAsync(
		IEnumerable<(int ProductId, string StorageCode)> selections,
		IReadOnlyCollection<int> excludedIds,
		CancellationToken cancellationToken)
	{
		var predicate = PredicateBuilder.New<StorageContent>(false);
		foreach (var selection in selections.Distinct())
			predicate = predicate.Or(content => content.ProductId == selection.ProductId &&
				content.StorageCode == selection.StorageCode);

		return repository.GetWithActiveLocksForUpdateAsync(
			Criteria<StorageContent>.New()
				.Where(predicate)
				.Where(content => content.Count > 0 && !excludedIds.Contains(content.Id)),
			cancellationToken);
	}

	private async Task<StorageContentExtractPolicyBase> GetPolicyAsync(CancellationToken cancellationToken)
	{
		var setting = (await settingsService.GetOrDefault<StorageContentSetting>(cancellationToken)).Data;
		return policyRegistry.GetBySystemName(setting.StorageContentExtractionPolicy);
	}

	private static int SubtractFrom(IEnumerable<StorageContent> contents,
		int requested, StorageMovementType movementType, DateTime utcNow, List<StorageLot> affected)
	{
		var remaining = requested;
		foreach (var content in contents)
		{
			if (remaining == 0) break;
			var count = Math.Min(content.GetAvailableQuantity(utcNow), remaining);
			if (count == 0) continue;
			content.Take(count, movementType, utcNow);
			affected.Add(new StorageLot(content.Id, content.ProductId, content.CurrencyId,
				content.BuyPrice, count, content.PurchaseDatetime));
			remaining -= count;
		}
		return remaining;
	}

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
