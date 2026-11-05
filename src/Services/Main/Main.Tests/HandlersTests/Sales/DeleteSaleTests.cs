using Tests.Stubs;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Interfaces.Events;
using Contracts;
using Tests.Extensions;
using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Sales;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public class DeleteSaleTests : IntegrationTest
{
	public DeleteSaleTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext SaleContext => GetContext<SaleTestContext>();

	private MessageBrokerStub Broker => (MessageBrokerStub)Scope.ServiceProvider
		.GetRequiredService<PublishEndpointAccessor>().Publisher;

	[Fact]
	public async Task DeleteSale_ValidSale_MarksSaleDeletedRestoresStockAndReversesTransaction()
	{
		var sale = await ReloadSale();
		var storageContentId = sale.Contents.Single().Details.Single().StorageContentId;
		var productId = sale.Contents.Single().ProductId;
		var transactionId = sale.TransactionId;
		var soldCount = SaleContext.SoldCount;
		var storageCountBeforeDelete = await StorageContentCount(storageContentId);
		var productStockBeforeDelete = await ProductStock(productId);

		var publishedBefore = Broker.PublishedMessages.Count;
		await Mediator.Send(new DeleteSaleCommand(sale.Id, sale.RowVersion), CancellationToken);

		var deletedSale = await ReloadSale();
		var storageContent = await Context
			.StorageContents
			.AsNoTracking()
			.SingleAsync(x => x.Id == storageContentId, CancellationToken);
		var productStockQuantity = await Context.GetProductStockQuantityAsync(productId, CancellationToken);
		var transaction = await Context
			.Transactions
			.AsNoTracking()
			.SingleAsync(x => x.Id == transactionId, CancellationToken);

		deletedSale.State.Should().Be(SaleState.Deleted);
		storageContent.Count.Should().Be(storageCountBeforeDelete + soldCount);
		productStockQuantity.Should().Be(productStockBeforeDelete + soldCount);
		transaction.IsReversed.Should().BeTrue();
		transaction.IsReversalApplied.Should().BeTrue();
		transaction.SourceType.Should().Be(TransactionSourceType.Sale);

		var published = Broker.PublishedMessages.Skip(publishedBefore).ToArray();
		published.OfType<SaleDeletedEvent>().Should().ContainSingle().Which.Id.Should().Be(sale.Id);
		published.OfType<SaleCreatedEvent>().Should().BeEmpty();
		published.OfType<SaleUpdatedEvent>().Should().BeEmpty();
	}

	[Fact]
	public async Task DeleteSale_WhenSaleAlreadyDeleted_DoesNothing()
	{
		var sale = await ReloadSale();
		var storageContentId = sale.Contents.Single().Details.Single().StorageContentId;
		var productId = sale.Contents.Single().ProductId;

		await Mediator.Send(new DeleteSaleCommand(sale.Id, sale.RowVersion), CancellationToken);

		var storageCountAfterFirstDelete = await StorageContentCount(storageContentId);
		var productStockAfterFirstDelete = await ProductStock(productId);

		await Mediator.Send(new DeleteSaleCommand(sale.Id, sale.RowVersion), CancellationToken);

		var deletedSale = await ReloadSale();
		var transaction = await Context
			.Transactions
			.AsNoTracking()
			.SingleAsync(x => x.Id == sale.TransactionId, CancellationToken);

		deletedSale.State.Should().Be(SaleState.Deleted);
		(await StorageContentCount(storageContentId)).Should().Be(storageCountAfterFirstDelete);
		(await ProductStock(productId)).Should().Be(productStockAfterFirstDelete);
		transaction.IsReversed.Should().BeTrue();
		transaction.IsReversalApplied.Should().BeTrue();
	}

	[Fact]
	public async Task DeleteSale_WhenSaleDoesNotExist_ThrowsSaleNotFoundException()
	{
		await Assert.ThrowsAsync<SaleNotFoundException>(() => Mediator.Send(
			new DeleteSaleCommand(Guid.NewGuid(), 1),
			CancellationToken));
	}

	[Fact]
	public async Task DeleteSale_WhenRowVersionIsInvalid_ThrowsInvalidRowVersionException()
	{
		var sale = await ReloadSale();
		var invalidRowVersion = sale.RowVersion + 1;

		var publishedBefore = Broker.PublishedMessages.Count;
		await Assert.ThrowsAsync<InvalidRowVersionException>(() => Mediator.Send(
			new DeleteSaleCommand(sale.Id, invalidRowVersion),
			CancellationToken));

		var saleAfterFailedDelete = await ReloadSale();
		var transaction = await Context
			.Transactions
			.AsNoTracking()
			.SingleAsync(x => x.Id == sale.TransactionId, CancellationToken);

		saleAfterFailedDelete.State.Should().Be(SaleState.Completed);
		transaction.IsReversed.Should().BeFalse();
		transaction.IsReversalApplied.Should().BeFalse();
		Broker.PublishedMessages.Skip(publishedBefore).OfType<SaleDeletedEvent>().Should().BeEmpty();
	}

	private Task<Sale> ReloadSale()
	{
		return Context
			.Sales
			.Include(x => x.Contents)
			.ThenInclude(x => x.Details)
			.AsNoTracking()
			.SingleAsync(x => x.Id == SaleContext.Sale.Id);
	}

	private async Task<int> StorageContentCount(int storageContentId)
	{
		var storageContent = await Context
			.StorageContents
			.AsNoTracking()
			.SingleAsync(x => x.Id == storageContentId);

		return storageContent.Count;
	}

	private Task<int> ProductStock(int productId) =>
		Context.GetProductStockQuantityAsync(productId, CancellationToken);
}
