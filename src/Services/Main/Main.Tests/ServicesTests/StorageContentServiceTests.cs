using System.Diagnostics;
using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Attributes;
using FluentAssertions;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Products;
using Main.Application.Handlers.StorageContents.EditContent;
using Main.Application.Handlers.StorageContents.SetToZeroContent;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Tests.DataBuilders.Storage;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.ServicesTests;

public class StorageContentServiceTests : IntegrationTest
{
	public StorageContentServiceTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
	}

	[Fact]
	public async Task PartialLock_UsesOnlyFreeQuantityAndContinuesIntoNextParty()
	{
		var first = NewContent(10);
		var second = NewContent(5, date: first.PurchaseDatetime.AddDays(1));
		await Add(first, second);
		await AddLock(first, 7);

		var result = await WithdrawProducts(new SubtractProductFromStorageItem(first.ProductId, first.StorageCode, 6));

		result.Select(lot => (lot.Id, lot.Count)).Should().Equal((first.Id, 3), (second.Id, 3));
		(await Count(first)).Should().Be(7);
		(await Count(second)).Should().Be(2);
	}

	[Fact]
	public async Task FullyLockedPreferredParty_FallsBackToAnotherPartyOnSameStorage()
	{
		var first = NewContent(10);
		var second = NewContent(5, date: first.PurchaseDatetime.AddDays(1));
		await Add(first, second);
		await AddLock(first, 10);

		var result = await Withdraw(new SubtractStorageContentItem(first.Id, 3));

		result.Should().ContainSingle().Which.Id.Should().Be(second.Id);
		(await Count(first)).Should().Be(10);
		(await Count(second)).Should().Be(2);
	}

	[Fact]
	public async Task SelectedParties_AreUsedBeforeFallbackForOtherRequests()
	{
		var first = NewContent(1);
		var second = NewContent(5, date: first.PurchaseDatetime.AddDays(1));
		await Add(first, second);

		var result = await Withdraw(new SubtractStorageContentItem(first.Id, 3),
			new SubtractStorageContentItem(second.Id, 3));

		result.Select(lot => (lot.Id, lot.Count)).Should().Equal((first.Id, 1), (second.Id, 3), (second.Id, 2));
		(await Count(first)).Should().Be(0);
		(await Count(second)).Should().Be(0);
	}

	[Fact]
	public async Task EmptySelectedParty_FallsBackOnlyToSameProductAndStorage()
	{
		var preferred = NewContent(0);
		var otherProduct = NewContent(10, product: 1);
		var otherStorage = NewContent(10, storage: 1);
		var fallback = NewContent(5);
		await Add(preferred, otherProduct, otherStorage, fallback);

		var result = await Withdraw(new SubtractStorageContentItem(preferred.Id, 3));

		result.Should().ContainSingle().Which.Id.Should().Be(fallback.Id);
		(await Count(fallback)).Should().Be(2);
		(await Count(otherProduct)).Should().Be(10);
		(await Count(otherStorage)).Should().Be(10);
	}

	[Fact]
	public async Task SufficientSelectedParty_DoesNotWaitForAnotherPartyLock()
	{
		var preferred = NewContent(5);
		var other = NewContent(10);
		await Add(preferred, other);
		var factory = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>();
		var locked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		async Task HoldOtherParty()
		{
			await using var transaction = factory.Create();
			await transaction.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (context, token) =>
			{
				await context.GetRequiredService<IRepository<StorageContent, int>>()
					.EnsureExistForUpdateAsync(other.Id, id => new StorageContentNotFoundException(id), ct: token);
				locked.TrySetResult();
				await release.Task.WaitAsync(token);
			}, CancellationToken);
		}
		var holder = HoldOtherParty();
		await locked.Task.WaitAsync(CancellationToken);
		var withdrawal = Withdraw(new SubtractStorageContentItem(preferred.Id, 3));
		try
		{
			var result = await withdrawal.WaitAsync(TimeSpan.FromSeconds(10), CancellationToken);
			result.Should().ContainSingle().Which.Id.Should().Be(preferred.Id);
		}
		finally
		{
			release.TrySetResult();
			await holder;
			await withdrawal;
		}

		(await Count(preferred)).Should().Be(2);
		(await Count(other)).Should().Be(10);
	}

	[Theory]
	[InlineData(StorageContentLockState.Released)]
	[InlineData(StorageContentLockState.Consumed)]
	[InlineData(StorageContentLockState.Expired)]
	public async Task ClosedLocks_DoNotReduceAvailability(StorageContentLockState state)
	{
		var content = NewContent(10);
		await Add(content);
		var item = StorageContentLock.Create(content.Id, 8, DateTime.UtcNow.AddHours(1));
		var now = DateTime.UtcNow;
		switch (state)
		{
			case StorageContentLockState.Released: item.Release(now); break;
			case StorageContentLockState.Consumed: item.Consume(now); break;
			case StorageContentLockState.Expired: item.Expire(item.ExpiresAt!.Value); break;
		}
		await Context.AddAsync(item, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await Withdraw(new SubtractStorageContentItem(content.Id, 10));

		(await Count(content)).Should().Be(0);
	}

	[Fact]
	public async Task ExpiredActiveLock_DoesNotRequireBackgroundCleanup()
	{
		var content = NewContent(10);
		await Add(content);
		var item = await AddLock(content, 8);
		Scope.ServiceProvider.GetRequiredService<TestTimeProvider>().SetUtcNow(item.ExpiresAt!.Value);

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(
			new GetAvailableProductsStockItem(content.ProductId, content.StorageCode)), CancellationToken);
		result.Stocks.Values.Should().Equal(10);
		await Withdraw(new SubtractStorageContentItem(content.Id, 10));

		(await Count(content)).Should().Be(0);
		(await Context.StorageContentLocks.AsNoTracking().SingleAsync(CancellationToken)).State.Should().Be(StorageContentLockState.Active);
	}

	[Fact]
	public async Task InsufficientFreeQuantity_RollsBackWholeBatch()
	{
		var first = NewContent(10);
		var second = NewContent(10, product: 1);
		await Add(first, second);
		await AddLock(second, 8);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Withdraw(
			new SubtractStorageContentItem(first.Id, 4), new SubtractStorageContentItem(second.Id, 3)));

		(await Count(first)).Should().Be(10);
		(await Count(second)).Should().Be(10);
		(await Context.ProductStocks.AsNoTracking().SumAsync(stock => stock.Quantity, CancellationToken)).Should().Be(20);
	}

	[Fact]
	public async Task SequentialWithdrawals_CannotSpendFreeQuantityTwice()
	{
		var content = NewContent(10);
		await Add(content);
		await AddLock(content, 6);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Execute(async (context, token) =>
		{
			var service = context.GetRequiredService<IStorageContentService>();
			await service.SubtractByContentsAsync([new(content.Id, 3)], StorageMovementType.Sale, token);
			await context.UnitOfWork.SaveChangesAsync(token);
			await service.SubtractByProductsAsync([new(content.ProductId, content.StorageCode, 2)], StorageMovementType.Sale, token);
		}));

		(await Count(content)).Should().Be(10);
	}

	[Fact]
	public async Task AvailableCounts_SeparateProductsAndStoragesAndReturnMissingAsZero()
	{
		var first = NewContent(10);
		var second = NewContent(4, storage: 1);
		await Add(first, second);
		await AddLock(first, 7);
		await AddLock(second, 2);
		var keys = new[]
		{
			new GetAvailableProductsStockItem(first.ProductId, first.StorageCode),
			new GetAvailableProductsStockItem(second.ProductId, second.StorageCode),
			new GetAvailableProductsStockItem(GetContext<ProductTestContext>().Products[1].Id, first.StorageCode)
		};

		var result = await Mediator.Send(new GetAvailableProductsStockQuery(keys), CancellationToken);

		keys.Select(key => result.Stocks[key]).Should().Equal(3, 2, 0);
	}

	[Fact]
	public async Task ZeroingLockedParty_IsRejected()
	{
		var content = NewContent(10);
		await Add(content);
		await AddLock(content, 7);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Mediator.Send(
			new SetToZeroContentCommand(content.Id, content.RowVersion), CancellationToken));

		(await Count(content)).Should().Be(10);
	}

	[Theory]
	[InlineData(6, false)]
	[InlineData(7, true)]
	public async Task EditingQuantity_RespectsHeldQuantity(int quantity, bool succeeds)
	{
		var content = NewContent(10);
		await Add(content);
		await AddLock(content, 7);
		var command = new EditStorageContentCommand(new()
		{
			[content.Id] = new ModelWithRowVersion<PatchStorageContentDto, uint>(
				new PatchStorageContentDto { Count = new PatchField<int> { IsSet = true, Value = quantity } }, content.RowVersion)
		});
		if (succeeds)
			await Mediator.Send(command, CancellationToken);
		else
			await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Mediator.Send(command, CancellationToken));

		(await Count(content)).Should().Be(succeeds ? quantity : 10);
	}

	[Fact]
	public async Task SavedRestoration_CanBeWithdrawnInSameTransaction()
	{
		var content = NewContent(0);
		await Add(content);

		await Execute(async (context, token) =>
		{
			content.IncreaseCount(5, StorageMovementType.SaleEditing);
			await context.UnitOfWork.SaveChangesAsync(token);
			await context.GetRequiredService<IStorageContentService>().SubtractByProductsAsync(
				[new SubtractProductFromStorageItem(content.ProductId, content.StorageCode, 3)], StorageMovementType.Sale, token);
			await context.UnitOfWork.SaveChangesAsync(token);
		});

		(await Count(content)).Should().Be(2);
	}

	[Fact]
	public async Task SavedNewLock_IsIncludedAndRolledBackWithFailedWithdrawal()
	{
		var content = NewContent(10);
		await Add(content);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Execute(async (context, token) =>
		{
			await context.UnitOfWork.AddAsync(StorageContentLock.Create(content.Id, 8), token);
			await context.UnitOfWork.SaveChangesAsync(token);
			await context.GetRequiredService<IStorageContentService>().SubtractByContentsAsync(
				[new SubtractStorageContentItem(content.Id, 3)], StorageMovementType.Sale, token);
		}));

		(await Count(content)).Should().Be(10);
		(await Context.StorageContentLocks.AsNoTracking().AnyAsync(CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public async Task SavedRelease_FreesQuantityWithinSameTransaction()
	{
		var content = NewContent(10);
		await Add(content);
		var item = await AddLock(content, 8);

		await Execute(async (context, token) =>
		{
			item.Release(Scope.ServiceProvider.GetRequiredService<TestTimeProvider>().GetUtcNow().UtcDateTime);
			await context.UnitOfWork.SaveChangesAsync(token);
			await context.GetRequiredService<IStorageContentService>().SubtractByContentsAsync(
				[new SubtractStorageContentItem(content.Id, 10)], StorageMovementType.Sale, token);
			await context.UnitOfWork.SaveChangesAsync(token);
		});

		(await Count(content)).Should().Be(0);
		(await Context.StorageContentLocks.AsNoTracking().SingleAsync(CancellationToken)).State.Should().Be(StorageContentLockState.Released);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task ConcurrentWithdrawals_CannotSpendTheSameFreeQuantity(bool byContentId)
	{
		var content = NewContent(10);
		await Add(content);
		await AddLock(content, 2);
		var factory = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>();
		var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var entered = 0;
		async Task<bool> Take()
		{
			await using var transaction = factory.Create();
			try
			{
				await transaction.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (context, token) =>
				{
					if (Interlocked.Increment(ref entered) == 2) ready.TrySetResult();
					await ready.Task.WaitAsync(token);
					var service = context.GetRequiredService<IStorageContentService>();
					if (byContentId)
						await service.SubtractByContentsAsync([new(content.Id, 7)], StorageMovementType.Sale, token);
					else
						await service.SubtractByProductsAsync([new(content.ProductId, content.StorageCode, 7)], StorageMovementType.Sale, token);
					await context.UnitOfWork.SaveChangesAsync(token);
				}, CancellationToken);
				return true;
			}
			catch (NotEnoughCountOnStorageException) { return false; }
		}

		var results = await Task.WhenAll(Take(), Take());

		results.Should().BeEquivalentTo([true, false]);
		(await Count(content)).Should().Be(3);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task ReservationCommittedWhileWaiting_IsReadAfterAcquiringPartyLock(bool byContentId)
	{
		var content = NewContent(10);
		await Add(content);
		var factory = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>();
		var reserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var commit = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var readerPid = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		async Task Reserve()
		{
			await using var transaction = factory.Create();
			await transaction.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (context, token) =>
			{
				await context.GetRequiredService<IRepository<StorageContent, int>>()
					.EnsureExistForUpdateAsync(content.Id, id => new StorageContentNotFoundException(id), ct: token);
				await context.UnitOfWork.AddAsync(StorageContentLock.Create(content.Id, 8), token);
			await context.UnitOfWork.SaveChangesAsync(token);
				await context.UnitOfWork.SaveChangesAsync(token);
				reserved.TrySetResult();
				await commit.Task.WaitAsync(token);
			}, CancellationToken);
		}
		async Task Read()
		{
			await using var transaction = factory.Create();
			await transaction.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (context, token) =>
			{
				var db = context.GetRequiredService<DContext>();
				readerPid.TrySetResult(((NpgsqlConnection)db.Database.GetDbConnection()).ProcessID);
				var service = context.GetRequiredService<IStorageContentService>();
				if (byContentId)
					await service.SubtractByContentsAsync([new(content.Id, 7)], StorageMovementType.Sale, token);
				else
					await service.SubtractByProductsAsync([new(content.ProductId, content.StorageCode, 7)], StorageMovementType.Sale, token);
				await context.UnitOfWork.SaveChangesAsync(token);
			}, CancellationToken);
		}
		var writer = Reserve();
		await reserved.Task.WaitAsync(CancellationToken);
		var reader = Read();
		var pid = await readerPid.Task.WaitAsync(CancellationToken);
		try
		{
			var timer = Stopwatch.StartNew();
			while (await Context.Database.SqlQuery<int>($"SELECT COUNT(*)::integer AS \"Value\" FROM pg_stat_activity WHERE pid = {pid} AND wait_event_type = 'Lock'")
				.SingleAsync(CancellationToken) == 0)
			{
				if (timer.Elapsed > TimeSpan.FromSeconds(10)) throw new TimeoutException("Withdrawal did not wait for the party lock.");
				await Task.Delay(10, CancellationToken);
			}
		}
		finally { commit.TrySetResult(); await writer; }

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => reader);
		(await Count(content)).Should().Be(10);
	}

	[Fact]
	public async Task SavedRestoration_FailedWithdrawalRollsBackRestoration()
	{
		var content = NewContent(0);
		await Add(content);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Execute(async (context, token) =>
		{
			content.IncreaseCount(5, StorageMovementType.SaleEditing);
			await context.UnitOfWork.SaveChangesAsync(token);
			await context.GetRequiredService<IStorageContentService>().SubtractByProductsAsync(
				[new SubtractProductFromStorageItem(content.ProductId, content.StorageCode, 6)], StorageMovementType.Sale, token);
		}));

		(await Count(content)).Should().Be(0);
		(await Context.ProductStocks.AsNoTracking().SumAsync(stock => stock.Quantity, CancellationToken)).Should().Be(0);
	}

	[Fact]
	public async Task ByProducts_StartsFromPolicyPartyAndContinuesOnSameStorage()
	{
		var first = NewContent(5, date: DateTime.UtcNow.AddDays(-2));
		var second = NewContent(7, date: DateTime.UtcNow.AddDays(-1));
		await Add(first, second);

		var result = await WithdrawProducts(new SubtractProductFromStorageItem(first.ProductId, first.StorageCode, 6));

		result.Select(lot => (lot.Id, lot.Count)).Should().Equal((first.Id, 5), (second.Id, 1));
		(await Count(first)).Should().Be(0);
		(await Count(second)).Should().Be(6);
	}

	[Fact]
	public async Task ByProducts_NoPartyOnStorage_ThrowsNotEnoughCount()
	{
		var content = NewContent(10);
		await Add(content);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() =>
			WithdrawProducts(new SubtractProductFromStorageItem(content.ProductId, "missing-storage", 1)));

		(await Count(content)).Should().Be(10);
	}

	[Theory]
	[InlineData(0, "storage", 1)]
	[InlineData(1, "", 1)]
	[InlineData(1, " ", 1)]
	[InlineData(1, "storage", 0)]
	[InlineData(1, "storage", -1)]
	public async Task ByProducts_InvalidRequest_ThrowsValidationException(int productId, string storageCode, int count)
	{
		await Assert.ThrowsAsync<ValidationException>(() => WithdrawProducts(new SubtractProductFromStorageItem(productId, storageCode, count)));
	}

	[Fact]
	public async Task DomainTake_RequiresLoadedLocksForMaterializedParty()
	{
		var content = NewContent(10);
		await Add(content);
		var unloaded = await Context.StorageContents.AsNoTracking().SingleAsync(item => item.Id == content.Id, CancellationToken);

		var take = () => unloaded.Take(1, StorageMovementType.Sale, DateTime.UtcNow);

		take.Should().Throw<InvalidOperationException>();
		unloaded.Count.Should().Be(10);
	}

	[Fact]
	public async Task DomainTake_RejectsReservedQuantityWithoutServiceValidation()
	{
		var content = NewContent(10);
		await Add(content);
		await AddLock(content, 6);

		await Execute(async (context, token) =>
		{
			var repository = context.GetRequiredService<IRepository<StorageContent, int>>();
			var party = await repository.GetWithActiveLocksForUpdateAsync(content.Id, cancellationToken: token);

			var take = () => party.Take(5, StorageMovementType.Sale, DateTime.UtcNow);

			take.Should().Throw<NotEnoughCountOnStorageException>();
			party.Count.Should().Be(10);
		});
	}

	private StorageContent NewContent(int count, int product = 0, int storage = 0, DateTime? date = null) =>
		new StorageContentBuilder(Faker).WithProductIds(GetContext<ProductTestContext>().Products[product].Id)
			.WithStorageCode(GetContext<StorageTestContext>().Storages.ElementAt(storage).Code)
			.WithCurrencyId(GetContext<CurrencyTestContext>().Currencies[0].Id).WithCount(count).WithBuyPrice(10m)
			.WithPurchaseDate(date ?? DateTime.UtcNow.AddDays(-1)).Build();

	private Task Add(params StorageContent[] contents) => Execute(async (context, token) =>
	{
		await context.UnitOfWork.AddRangeAsync(contents, token);
		await context.UnitOfWork.SaveChangesAsync(token);
	});

	private async Task<StorageContentLock> AddLock(StorageContent content, int quantity)
	{
		var item = StorageContentLock.Create(content.Id, quantity, DateTime.UtcNow.AddHours(1));
		await Context.AddAsync(item, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		return item;
	}

	private Task Execute(Func<IApplicationTransactionContext, CancellationToken, Task> action) =>
		Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>()
			.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(action, CancellationToken);

	private Task<IReadOnlyList<StorageLot>> Withdraw(params SubtractStorageContentItem[] items) =>
		ExecuteWithdrawal((service, token) => service.SubtractByContentsAsync(items, StorageMovementType.Sale, token));

	private Task<IReadOnlyList<StorageLot>> WithdrawProducts(params SubtractProductFromStorageItem[] items) =>
		ExecuteWithdrawal((service, token) => service.SubtractByProductsAsync(items, StorageMovementType.Sale, token));

	private Task<IReadOnlyList<StorageLot>> ExecuteWithdrawal(
		Func<IStorageContentService, CancellationToken, Task<IReadOnlyList<StorageLot>>> action) =>
		Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>()
			.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (context, token) =>
			{
				var result = await action(context.GetRequiredService<IStorageContentService>(), token);
				await context.UnitOfWork.SaveChangesAsync(token);
				return result;
			}, CancellationToken);

	private async Task<int> Count(StorageContent content) =>
		(await Context.StorageContents.AsNoTracking().SingleAsync(item => item.Id == content.Id, CancellationToken)).Count;
}
