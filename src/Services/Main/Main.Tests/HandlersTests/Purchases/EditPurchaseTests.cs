using Enums;
using Application.Common.Interfaces.Persistence;
using Application.Common.Services;
using Attributes;
using Contracts;
using FluentAssertions;
using FluentValidation.TestHelper;
using Main.Entities;
using Main.Entities.Balance;
using Main.Entities.Balance.Balance;
using Main.Entities.Product.Stock;
using Main.Entities.Purchase;
using Main.Entities.Purchase.Purchase;
using Main.Entities.Storage;
using Main.Entities.Storage.Storage;
using Main.Modules.Purchase.Application.Dtos.Purchase;
using Main.Modules.Purchase.Application.Handlers.Purchases.EditPurchase;
using Main.Modules.Storage.Abstractions.Interfaces;
using Main.Modules.Storage.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Purchase;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Purchases;

public class EditPurchaseTests : IntegrationTest
{
	public EditPurchaseTests(CombinedContainerFixture fixture) : base(fixture)
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
	public async Task EditPurchase_ValidContent_UpdatesPurchaseStorageAndTransaction()
	{
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var purchase = PurchaseContext.Purchase;
		var product = PurchaseContext.Product;
		var content = purchase.Contents.Single();
		var oldTransactionId = purchase.TransactionId;

		var editCommand = new EditPurchaseCommand(
			[
				new EditPurchaseDto
				{
					Id = content.Id,
					ProductId = product.Id,
					Count = 5,
					Price = 20m,
					Comment = "updated"
				}
			],
			purchase.Id,
			currency.Id,
			"edited purchase",
			DateTime.UtcNow);

		var publishedBefore = Broker.PublishedMessages.Count;
		await Mediator.Send(editCommand, CancellationToken);

		var updatedPurchase =
			await Context.Set<Purchase>()
				.Include(x => x.Contents)
				.AsNoTracking()
				.SingleAsync(CancellationToken);
		var updatedContent = updatedPurchase.Contents.Single();
		var storageContent = await Context
			.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == updatedContent.StorageContentId, CancellationToken);
		var oldTransaction = await Context
			.Set<Transaction>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == oldTransactionId, CancellationToken);
		var newTransaction = await Context
			.Set<Transaction>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == updatedPurchase.TransactionId, CancellationToken);
		var updatedProductStockQuantity =
			await Context.GetProductStockQuantityAsync(product.Id, CancellationToken);

		updatedPurchase.Comment.Should().Be(editCommand.Comment);
		updatedPurchase.CurrencyId.Should().Be(currency.Id);
		updatedContent.Count.Should().Be(5);
		updatedContent.Price.Should().Be(20m);
		updatedContent.TotalSum.Should().Be(100m);
		updatedContent.Comment.Should().Be("updated");
		storageContent.Count.Should().Be(5);
		storageContent.BuyPrice.Should().Be(20m);
		oldTransaction.IsReversed.Should().BeTrue();
		oldTransaction.IsReversalApplied.Should().BeTrue();
		newTransaction.Amount.Should().Be(100m);
		updatedProductStockQuantity.Should().Be(5);

		var published = Broker.PublishedMessages.Skip(publishedBefore).ToArray();
		published.OfType<PurchaseUpdatedEvent>().Should().ContainSingle().Which.Id.Should().Be(purchase.Id);
		published.OfType<PurchaseCreatedEvent>().Should().BeEmpty();
		published.OfType<PurchaseDeletedEvent>().Should().BeEmpty();
	}

	[Fact]
	public async Task EditPurchase_AddAndRemoveContent_UpdatesPurchaseAndStorage()
	{
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var products = GetContext<ProductTestContext>().Products.Take(2).ToList();
		var purchase = PurchaseContext.Purchase;
		var removedStorageContentId = purchase.Contents.Single().StorageContentId;

		var editCommand = new EditPurchaseCommand(
			[
				new EditPurchaseDto
				{
					ProductId = products[1].Id,
					Count = 3,
					Price = 15m
				}
			],
			purchase.Id,
			currency.Id,
			null,
			DateTime.UtcNow);

		await Mediator.Send(editCommand, CancellationToken);

		var updatedPurchase =
			await Context.Set<Purchase>()
				.Include(x => x.Contents)
				.AsNoTracking()
				.SingleAsync(CancellationToken);
		var contents = await Context.Set<StorageContent>().AsNoTracking().ToListAsync(CancellationToken);
		var removedProductStockQuantity =
			await Context.GetProductStockQuantityAsync(products[0].Id, CancellationToken);
		var addedProductStockQuantity =
			await Context.GetProductStockQuantityAsync(products[1].Id, CancellationToken);

		updatedPurchase.Contents.Should().ContainSingle(x => x.ProductId == products[1].Id);
		contents.Single(x => x.Id == removedStorageContentId).Count.Should().Be(0);
		contents.Single(x => x.ProductId == products[1].Id).Count.Should().Be(3);
		removedProductStockQuantity.Should().Be(0);
		addedProductStockQuantity.Should().Be(3);
	}

	[Fact]
	public async Task
		EditPurchase_WhenPartOfStorageContentAlreadyUsed_DecreaseSubtractsOnlyDeltaFromRemainingStock()
	{
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var purchase = PurchaseContext.Purchase;
		var product = PurchaseContext.Product;
		var content = purchase.Contents.Single();
		var storageContentId = content.StorageContentId;
		content.SetCount(10);
		PurchaseContext.StorageContent.IncreaseCount(8, StorageMovementType.PurchaseEditing);
		var stock = await Context.Set<ProductStock>()
			.SingleAsync(
				item => item.ProductId == product.Id &&
					item.StorageCode == PurchaseContext.StorageContent.StorageCode,
				CancellationToken);
		stock.SetQuantity(stock.Quantity + 8);
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();

		await Scope.ServiceProvider
			.GetRequiredService<IApplicationTransaction>()
			.WithSettings(TransactionalAttribute.ReadCommitted(0, 0))
			.ExecuteAsync(
				async (context, token) =>
				{
					await context.GetRequiredService<IStorageContentService>()
						.SubtractByContentsAsync(
							[new SubtractStorageContentItem(storageContentId, 5)],
							StorageMovementType.Sale,
							token);
					await context.UnitOfWork.SaveChangesAsync(token);
				},
				CancellationToken);

		await Mediator.Send(
			new EditPurchaseCommand(
				[
					new EditPurchaseDto
					{
						Id = content.Id,
						ProductId = product.Id,
						Count = 8,
						Price = 10m
					}
				],
				purchase.Id,
				currency.Id,
				null,
				DateTime.UtcNow),
			CancellationToken);

		var updatedPurchaseContent =
			await Context.Set<PurchaseContent>().AsNoTracking().SingleAsync(CancellationToken);
		var updatedStorageContent = await Context
			.Set<StorageContent>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == storageContentId, CancellationToken);
		var updatedProductStockQuantity =
			await Context.GetProductStockQuantityAsync(product.Id, CancellationToken);

		updatedPurchaseContent.Count.Should().Be(8);
		updatedStorageContent.Count.Should().Be(3);
		updatedProductStockQuantity.Should().Be(3);
	}

}

public class EditPurchaseValidationTests : ValidationTest<EditPurchaseValidation, EditPurchaseCommand>
{
	protected override EditPurchaseValidation CreateValidator()
		=> new(new OperationDatePolicy(TimeProvider.System, new UserContextMock()));

	[Fact]
	public async Task EditPurchase_WithDuplicateContentIds_FailsValidation()
	{
		var result = await ValidateAsync(
			new EditPurchaseCommand(
				[
					new EditPurchaseDto { Id = 1, ProductId = 1, Count = 1, Price = 10m },
					new EditPurchaseDto { Id = 1, ProductId = 1, Count = 1, Price = 10m }
				],
				Guid.NewGuid(),
				1,
				null,
				DateTime.UtcNow));
		result.ShouldHaveValidationErrorFor(x => x.Content)
			.WithErrorCode(PurchaseContentDuplicateIdsNotAllowedMessage.Instance.MessageKey)
			.WithCustomState(PurchaseContentDuplicateIdsNotAllowedMessage.Instance)
			.Only();
	}
}
