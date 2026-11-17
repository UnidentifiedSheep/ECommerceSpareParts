using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Enums;
using FluentValidation;
using LinqKit;
using Main.Entities.Storage.Exceptions;
using Main.Entities.Storage.Settings;
using Main.Entities.Storage.Storage;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Storage.Abstractions.Interfaces;
using Main.Modules.Storage.Abstractions.Models;
using Main.Modules.Storage.Application.Dtos.Storage;
using Main.Modules.Storage.Application.Extensions;
using Main.Modules.Storage.Application.Handlers.StorageContents.AddContent;
using Main.Modules.Storage.Application.Handlers.StorageContents.RestoreContent;
using Main.Modules.Storage.Application.Models;
using Main.Modules.Storage.Application.NamedObjects.StorageContentExtractPolicies;
using MediatR;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Storage.Application.Services.Storage;

public sealed class StorageContentService(
	IRepository<StorageContent, int> repository,
	ISettingsService settingsService,
	INamedObjectRegistry<StorageContentExtractPolicyBase> policyRegistry,
	IValidator<IReadOnlyCollection<SubtractStorageContentItem>> contentValidator,
	IValidator<IReadOnlyCollection<SubtractProductFromStorageItem>> productValidator,
	IUnitOfWork unitOfWork,
	ISender sender,
	ICurrencyConverter currencyConverter,
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

			EnsureEnough(
				request.Key.ProductId,
				request.Count,
				remaining);
		}

		return affected;
	}

	public async Task<IReadOnlyList<StoredLot>> AddAsync(
		IReadOnlyCollection<NewStorageLot> lots,
		string storageCode,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
	{
		var result = await sender.Send(
			new AddContentCommand(
				lots.Select(lot => new NewStorageContentDto
					{
						ProductId = lot.ProductId,
						CurrencyId = lot.CurrencyId,
						BuyPrice = lot.BuyPrice,
						Count = lot.Count,
						PurchaseDate = lot.PurchaseDate
					})
					.ToArray(),
				storageCode,
				movementType),
			cancellationToken);

		return result.StorageContents.Select(ToStoredLot).ToArray();
	}

	public async Task RestoreAsync(
		IReadOnlyCollection<RestoreStorageLot> lots,
		StorageMovementType movementType,
		CancellationToken cancellationToken = default)
		=> await sender.Send(
			new RestoreContentCommand(
				lots.Select(lot => new RestoreContentItem(
						lot.StorageContentId,
						lot.ProductId,
						lot.CurrencyId,
						lot.BuyPrice,
						lot.Count))
					.ToArray(),
				movementType),
			cancellationToken);

	public async Task<IReadOnlyDictionary<int, StoredLot>> LockAsync(
		IReadOnlyCollection<int> storageContentIds,
		CancellationToken cancellationToken = default)
	{
		var contents = await repository.EnsureExistsForUpdateAsync(
			storageContentIds,
			missing => new StorageContentNotFoundException(missing[0]),
			Criteria<StorageContent>.New().OrderByAsc(content => content.Id),
			cancellationToken);

		return contents.ToDictionary(pair => pair.Key, pair => ToStoredLot(pair.Value));
	}

	public async Task UpdatePurchaseInfoAsync(
		int storageContentId,
		int currencyId,
		DateTime purchaseDate,
		decimal buyPrice,
		CancellationToken cancellationToken = default)
	{
		var contents = await repository.EnsureExistsForUpdateAsync(
			[storageContentId],
			missing => new StorageContentNotFoundException(missing[0]),
			Criteria<StorageContent>.New(),
			cancellationToken);
		var content = contents[storageContentId];

		content.SetCurrencyId(currencyId);
		content.SetPurchaseDate(purchaseDate);
		content.SetBuyPrice(
			buyPrice,
			await currencyConverter.ConvertToBaseAsync(
				buyPrice,
				currencyId,
				cancellationToken));
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
			preferredIds,
			cancellationToken: cancellationToken);

		var utcNow = timeProvider.GetUtcNow().UtcDateTime;
		var affected = new List<StorageLot>();
		var outstanding = new List<(StorageContent Content, int Requested, int Remaining)>();
		foreach (var (id, count) in requests)
		{
			var content = preferred[id];
			var remaining = SubtractFrom(
				[content],
				count,
				movementType,
				utcNow,
				affected);
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
			var candidates = preferred.Values
				.Concat(additional)
				.Where(candidate =>
					candidate.ProductId == content.ProductId &&
					candidate.StorageCode == content.StorageCode &&
					candidate.Id != content.Id)
				.AsQueryable();
			var shortage = SubtractFrom(
				policy.Apply(candidates).ThenBy(candidate => candidate.Id),
				remaining,
				movementType,
				utcNow,
				affected);
			EnsureEnough(
				content.ProductId,
				requested,
				shortage);
		}

		return affected;
	}

	private static StoredLot ToStoredLot(StorageContent content)
		=> new(
			content.Id,
			content.ProductId,
			content.Count,
			content.CurrencyId,
			content.BuyPrice,
			content.PurchaseDatetime);

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

	private static int SubtractFrom(
		IEnumerable<StorageContent> contents,
		int requested,
		StorageMovementType movementType,
		DateTime utcNow,
		List<StorageLot> affected)
	{
		var remaining = requested;
		foreach (var content in contents)
		{
			if (remaining == 0) break;
			var count = Math.Min(content.GetAvailableQuantity(utcNow), remaining);
			if (count == 0) continue;
			content.Take(
				count,
				movementType,
				utcNow);
			affected.Add(
				new StorageLot(
					content.Id,
					content.ProductId,
					content.CurrencyId,
					content.BuyPrice,
					count,
					content.PurchaseDatetime));
			remaining -= count;
		}

		return remaining;
	}

	private void EnsureTransaction()
	{
		if (!unitOfWork.HasActiveTransaction)
			throw new InvalidOperationException("Storage content changes require an active transaction.");
	}

	private static void EnsureEnough(
		int productId,
		int requested,
		int remaining)
	{
		if (remaining > 0)
			throw new NotEnoughCountOnStorageException(
				productId,
				requested - remaining,
				requested);
	}
}
