using Enums;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Contracts;
using FluentAssertions;
using Main.Entities.Balance;
using Main.Entities.Balance.Balance;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Main.Entities.Purchase.Exceptions;
using Main.Entities.Purchase.Purchase;
using Main.Entities.Storage;
using Main.Entities.Storage.Exceptions;
using Main.Entities.Storage.Storage;
using Main.Modules.Purchase.Application.Handlers.Purchases.DeletePurchase;
using Main.Modules.Storage.Abstractions.Interfaces;
using Main.Modules.Storage.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Test;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Purchase;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Purchases;

public class DeletePurchaseTests : IntegrationTest
{
	public DeletePurchaseTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<PurchaseTestContext>();
		RegisterBasicContext<ProductMeasurementsTestContext>();
	}

	private PurchaseTestContext PurchaseContext => GetContext<PurchaseTestContext>();

	private MessageBrokerStub Broker
		=> (MessageBrokerStub)Scope.ServiceProvider
			.GetRequiredService<PublishEndpointAccessor>()
			.Publisher;

	[Fact]
	public async Task DeletePurchase_ValidPurchase_RemovesPurchaseAndRevertsStockAndTransaction()
	{
		var purchase = PurchaseContext.Purchase;
		var content = purchase.Contents.Single();
		var storageContentId = content.StorageContentId;
		var productId = content.ProductId;
		var transactionId = purchase.TransactionId;

		var publishedBefore = Broker.PublishedMessages.Count;
		await Mediator.Send(new DeletePurchaseCommand(purchase.Id), CancellationToken);

		var purchaseExists =
			await Context.Set<Purchase>()
				.AsNoTracking()
				.AnyAsync(x => x.Id == purchase.Id, CancellationToken);
		var purchaseContentExists = await Context
			.Set<PurchaseContent>()
			.AsNoTracking()
			.AnyAsync(x => x.PurchaseId == purchase.Id, CancellationToken);
		var storageContent = await Context
			.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == storageContentId, CancellationToken);
		var productStockQuantity = await Context.GetProductStockQuantityAsync(productId, CancellationToken);
		var transaction = await Context
			.Set<Transaction>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == transactionId, CancellationToken);

		purchaseExists.Should().BeFalse();
		purchaseContentExists.Should().BeFalse();
		storageContent.Count.Should().Be(0);
		productStockQuantity.Should().Be(0);
		transaction.IsReversed.Should().BeTrue();
		transaction.IsReversalApplied.Should().BeTrue();

		var published = Broker.PublishedMessages.Skip(publishedBefore).ToArray();
		published.OfType<PurchaseDeletedEvent>().Should().ContainSingle().Which.Id.Should().Be(purchase.Id);
		published.OfType<PurchaseCreatedEvent>().Should().BeEmpty();
		published.OfType<PurchaseUpdatedEvent>().Should().BeEmpty();
	}

	[Fact]
	public async Task DeletePurchase_WhenPurchaseDoesNotExist_ThrowsPurchaseNotFoundException()
	{
		var purchaseId = Guid.NewGuid();

		await Assert.ThrowsAsync<PurchaseNotFoundException>(() => Mediator.Send(
			new DeletePurchaseCommand(purchaseId),
			CancellationToken));
	}

	[Fact]
	public async Task DeletePurchase_WhenPurchasedStockAlreadyPartiallyUsed_DoesNotPersistPartialChanges()
	{
		var purchase = PurchaseContext.Purchase;
		var content = purchase.Contents.Single();
		var storageContentId = content.StorageContentId;
		var transactionId = purchase.TransactionId;

		await Scope.ServiceProvider
			.GetRequiredService<IApplicationTransaction>()
			.WithSettings(TransactionalAttribute.ReadCommitted(0, 0))
			.ExecuteAsync(
				async (context, token) =>
				{
					await context.GetRequiredService<IStorageContentService>()
						.SubtractByContentsAsync(
							[new SubtractStorageContentItem(storageContentId, 1)],
							StorageMovementType.Sale,
							token);
					await context.UnitOfWork.SaveChangesAsync(token);
				},
				CancellationToken);

		var publishedBefore = Broker.PublishedMessages.Count;
		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Mediator.Send(
			new DeletePurchaseCommand(purchase.Id),
			CancellationToken));

		var purchaseExists =
			await Context.Set<Purchase>()
				.AsNoTracking()
				.AnyAsync(x => x.Id == purchase.Id, CancellationToken);
		var storageContent = await Context
			.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == storageContentId, CancellationToken);
		var productStockQuantity =
			await Context.GetProductStockQuantityAsync(content.ProductId, CancellationToken);
		var transaction = await Context
			.Set<Transaction>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == transactionId, CancellationToken);

		purchaseExists.Should().BeTrue();
		storageContent.Count.Should().Be(1);
		productStockQuantity.Should().Be(1);
		transaction.IsReversed.Should().BeFalse();
		transaction.IsReversalApplied.Should().BeFalse();
		Broker.PublishedMessages.Skip(publishedBefore).OfType<PurchaseDeletedEvent>().Should().BeEmpty();
	}
}

public class DeletePurchaseValidationTests : ValidationTest<DeletePurchaseValidation, DeletePurchaseCommand>
{
	[Fact]
	public async Task DeletePurchase_WhenPurchaseIdIsEmpty_FailsValidation()
	{
		var result = await ValidateAsync(
			new DeletePurchaseCommand(Guid.Empty));
		result.ShouldHaveValidationErrorFor(x => x.PurchaseId);
	}
}
