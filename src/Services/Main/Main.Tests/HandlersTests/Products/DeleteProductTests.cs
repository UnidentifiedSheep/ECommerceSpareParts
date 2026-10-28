using System.Text.Json;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Contracts.Products;
using Contracts.ProductEnrichment;
using Domain.CommonEntities.Job;
using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Products;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.DeleteProductImages;
using Main.Entities.Exceptions;
using Main.Entities.Order;
using Main.Entities.Product;
using Main.Enums.Products;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using MediatR;
using Tests.DataBuilders.Organization;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Sale;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Products;

public class DeleteProductTests : IntegrationTest
{
	public DeleteProductTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
		RegisterBasicContext<CurrencyTestContext>();
	}

	private Product Product => GetContext<ProductTestContext>().Products[0];
	private Product Other => GetContext<ProductTestContext>().Products[1];
	private Guid UserId => GetContext<UserContextTestContext>().SystemUser.Id;
	private int CurrencyId => GetContext<CurrencyTestContext>().Currencies[0].Id;
	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;
	private S3StorageServiceStub S3 => Sp.GetRequiredService<S3StorageServiceStub>();

	[Fact]
	public async Task UnusedProduct_DeletesDependentsAndQueuesImagesWithoutDeletingFiles()
	{
		var characteristic = await new CharacteristicBuilder(Faker).BuildAndAddToDb(Context, CancellationToken);
		var image = ProductImage.Create(Product.Id, ".jpg");
		var secondImage = ProductImage.Create(Product.Id, ".png");
		var otherImage = ProductImage.Create(Other.Id, ".jpg");
		Product.AddEan("1234567890123");
		await Context.AddRangeAsync([image, secondImage, otherImage,
			ProductCharacteristic.Create(Product.Id, characteristic.Id, "value"),
			new ProductSizeBuilder(Faker).WithProductId(Product.Id).Build(),
			new ProductWeightBuilder(Faker).WithProductId(Product.Id).Build(),
			ProductContent.Create(Product.Id, Other.Id, 2), Main.Entities.Cart.Cart.Create(UserId, Product.Id, 1)], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		S3.SetFile("images", image.StorageKey, "image");
		S3.SetFile("images", secondImage.StorageKey, "second image");

		var id = Product.Id;
		using var deletionScope = Sp.CreateScope();
		await deletionScope.ServiceProvider.GetRequiredService<IMediator>()
			.Send(new DeleteProductCommand(id, Product.RowVersion), CancellationToken);

		(await Context.Products.AsNoTracking().AnyAsync(item => item.Id == id, CancellationToken)).Should().BeFalse();
		(await Context.ProductEans.AsNoTracking().AnyAsync(item => item.ProductId == id, CancellationToken)).Should().BeFalse();
		(await Context.ProductCharacteristics.AsNoTracking().AnyAsync(item => item.ProductId == id, CancellationToken)).Should().BeFalse();
		(await Context.ProductSizes.AsNoTracking().AnyAsync(item => item.ProductId == id, CancellationToken)).Should().BeFalse();
		(await Context.ProductWeights.AsNoTracking().AnyAsync(item => item.ProductId == id, CancellationToken)).Should().BeFalse();
		(await Context.ProductContents.AsNoTracking().AnyAsync(item => item.ParentProductId == id, CancellationToken)).Should().BeFalse();
		(await Context.Carts.AsNoTracking().AnyAsync(item => item.ProductId == id, CancellationToken)).Should().BeFalse();
		(await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken)).Should().ContainSingle().Which.StorageKey.Should().Be(otherImage.StorageKey);
		(await Context.Products.AsNoTracking().AnyAsync(item => item.Id == Other.Id, CancellationToken)).Should().BeTrue();
		(await Context.Characteristics.AsNoTracking().AnyAsync(item => item.Id == characteristic.Id, CancellationToken)).Should().BeTrue();
		var job = (await Context.Set<Job>().AsNoTracking()
			.Where(item => item.SystemName == DeleteProductImagesLrt.Name).ToListAsync(CancellationToken))
			.Should().ContainSingle("deletion must persist a cleanup job with the image keys").Which;
		var input = JsonSerializer.Deserialize<DeleteProductImagesInputState>(job.State)!;
		input.ProductId.Should().Be(id);
		input.BucketName.Should().Be("images");
		input.StorageKeys.Should().BeEquivalentTo([image.StorageKey, secondImage.StorageKey]);
		(await S3.DownloadFileAsync("images", image.StorageKey, CancellationToken)).IsSuccess.Should().BeTrue();
		(await S3.DownloadFileAsync("images", secondImage.StorageKey, CancellationToken)).IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Delete_InvalidatesProductAndCrossCaches()
	{
		await Context.AddAsync(ProductCross.Create(Product.Id, Other.Id), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		await provider.GetProductOrSetAsync(Product.Id, CancellationToken);
		await provider.GetProductOrSetAsync(Other.Id, CancellationToken);
		(await provider.GetProductCrossesAsync(Other.Id, null, CancellationToken)).Should().Contain(Product.Id);

		await Delete();

		(await provider.GetProductAsync(Product.Id, CancellationToken)).Should().BeNull();
		(await provider.GetProductAsync(Other.Id, CancellationToken)).Should().NotBeNull();
		(await provider.GetProductCrossesAsync(Other.Id, null, CancellationToken)).Should().BeEmpty();
		(await Context.ProductCrosses.AsNoTracking().ToListAsync(CancellationToken)).Should().BeEmpty();
		(await Context.Set<Job>().AsNoTracking().AnyAsync(item => item.SystemName == DeleteProductImagesLrt.Name, CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public async Task Delete_PublishesTombstoneForSearchConsumers()
	{
		var publisher = (MessageBrokerStub)Scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
		var publishedBefore = publisher.PublishedMessages.Count;

		await Delete();

		publisher.PublishedMessages.Skip(publishedBefore).OfType<ProductsUpdatedEvent>().SelectMany(item => item.Products)
			.Should().ContainSingle(item => item.Id == Product.Id && item.IsDeleted);
	}

	[Fact]
	public async Task Delete_DetachesPairAndCatalogueCandidateAndPublishesMappingChange()
	{
		Other.SetPair(Product.Id);
		var candidate = await new CatalogueCandidateBuilder(Faker).WithProductId(Product.Id)
			.WithProducerId(Product.ProducerId).BuildAndAddToDb(Context, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		var publisher = (MessageBrokerStub)Scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
		var publishedBefore = publisher.PublishedMessages.Count;

		using var deletionScope = Sp.CreateScope();
		await deletionScope.ServiceProvider.GetRequiredService<IMediator>()
			.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), CancellationToken);

		(await Context.Products.AsNoTracking().SingleAsync(item => item.Id == Other.Id, CancellationToken)).PairId.Should().BeNull();
		(await Context.CatalogueCandidates.AsNoTracking().SingleAsync(item => item.Id == candidate.Id, CancellationToken)).ProductId.Should().BeNull();
		publisher.PublishedMessages.Skip(publishedBefore).OfType<CatalogueCandidateUpdatedEvent>()
			.Should().ContainSingle(item => item.Candidate.Id == candidate.Id && item.Candidate.MappedProductId == null);
	}

	[Fact]
	public async Task Delete_WithTrackedPair_DetachesPairWithoutConcurrencyConflict()
	{
		Other.SetPair(Product.Id);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();

		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		await transaction.WithSettings(TransactionalAttribute.Serializable(30, 0)).ExecuteAsync(
			async (_, ct) => await Mediator.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), ct),
			CancellationToken);

		(await Context.Products.AsNoTracking().AnyAsync(item => item.Id == Product.Id, CancellationToken))
			.Should().BeFalse();
		(await Context.Products.AsNoTracking().SingleAsync(item => item.Id == Other.Id, CancellationToken))
			.PairId.Should().BeNull();
	}

	[Fact]
	public async Task MissingProduct_ThrowsNotFound()
	{
		var action = () => Mediator.Send(new DeleteProductCommand(int.MaxValue, 0), CancellationToken);
		await action.Should().ThrowAsync<ProductNotFoundException>();
	}

	[Fact]
	public async Task StaleVersion_PreservesUpdatedProduct()
	{
		var version = Product.RowVersion;
		Product.SetDescription("updated since opening the deletion dialog");
		await Context.SaveChangesAsync(CancellationToken);
		var action = () => Mediator.Send(new DeleteProductCommand(Product.Id, version), CancellationToken);
		await action.Should().ThrowAsync<InvalidRowVersionException>();
		(await Context.Products.AsNoTracking().SingleAsync(item => item.Id == Product.Id, CancellationToken))
			.Description.Should().Be("updated since opening the deletion dialog");
	}

	[Theory]
	[InlineData(ProductDeletionBlocker.Stock)]
	[InlineData(ProductDeletionBlocker.StorageContent)]
	[InlineData(ProductDeletionBlocker.Sale)]
	[InlineData(ProductDeletionBlocker.Purchase)]
	[InlineData(ProductDeletionBlocker.Order)]
	[InlineData(ProductDeletionBlocker.Reservation)]
	[InlineData(ProductDeletionBlocker.Composition)]
	public async Task ReferencedProduct_IsPreservedWithoutEventsOrCleanup(ProductDeletionBlocker blocker)
	{
		await AddBlocker(blocker);
		var image = ProductImage.Create(Product.Id, ".jpg");
		await Context.AddAsync(image, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		await provider.GetProductOrSetAsync(Product.Id, CancellationToken);
		var publisher = (MessageBrokerStub)Scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
		var publishedBefore = publisher.PublishedMessages.Count;

		var action = () => Delete();

		(await action.Should().ThrowAsync<ProductDeletionNotAllowedException>()).Which.Blocker.Should().Be(blocker);
		(await Context.Products.AsNoTracking().AnyAsync(item => item.Id == Product.Id, CancellationToken)).Should().BeTrue();
		(await Context.ProductImages.AsNoTracking().AnyAsync(item => item.ProductId == Product.Id, CancellationToken)).Should().BeTrue();
		(await provider.GetProductAsync(Product.Id, CancellationToken)).Should().NotBeNull();
		(await Context.Set<Job>().AsNoTracking().AnyAsync(item => item.SystemName == DeleteProductImagesLrt.Name, CancellationToken)).Should().BeFalse();
		publisher.PublishedMessages.Skip(publishedBefore).Should().BeEmpty();
	}

	[Fact]
	public async Task OuterTransactionRollback_PreservesProductAndDoesNotQueueCleanupOrPublishDeletion()
	{
		var image = ProductImage.Create(Product.Id, ".jpg");
		await Context.AddAsync(image, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		S3.SetFile("images", image.StorageKey, "image");
		var publisher = (MessageBrokerStub)Scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
		var publishedBefore = publisher.PublishedMessages.Count;
		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var action = () => transaction.WithSettings(TransactionalAttribute.Serializable(30, 1)).ExecuteAsync(async (_, ct) =>
		{
			await Mediator.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), ct);
			throw new InvalidOperationException("Rollback test");
		}, CancellationToken);

		await action.Should().ThrowAsync<InvalidOperationException>();

		using var verification = Sp.CreateScope();
		var db = verification.ServiceProvider.GetRequiredService<DContext>();
		(await db.Products.AnyAsync(item => item.Id == Product.Id, CancellationToken)).Should().BeTrue();
		(await db.ProductImages.AnyAsync(item => item.ProductId == Product.Id, CancellationToken)).Should().BeTrue();
		(await db.Set<Job>().AnyAsync(item => item.SystemName == DeleteProductImagesLrt.Name, CancellationToken)).Should().BeFalse();
		publisher.PublishedMessages.Skip(publishedBefore).OfType<ProductsUpdatedEvent>().SelectMany(item => item.Products)
			.Should().NotContain(item => item.Id == Product.Id && item.IsDeleted);
		(await S3.DownloadFileAsync("images", image.StorageKey, CancellationToken)).IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task ConcurrentCompositionInsert_CannotAttachToDeletedProduct()
	{
		using var insertionScope = Sp.CreateScope();
		var insertionDb = insertionScope.ServiceProvider.GetRequiredService<DContext>();
		var content = ProductContent.Create(Other.Id, Product.Id, 1);
		insertionDb.ProductContents.Add(content);
		Task<int>? insertion = null;
		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();

		await transaction.WithSettings(TransactionalAttribute.Serializable(30, 1)).ExecuteAsync(async (_, ct) =>
		{
			await Mediator.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), ct);
			// A second connection tries to insert a reference while deletion is still uncommitted.
			insertion = insertionDb.SaveChangesAsync(ct);
			insertion.IsCompleted.Should().BeFalse();
		}, CancellationToken);

		var action = async () => await insertion!;
		(await action.Should().ThrowAsync<DbUpdateException>()).Which.InnerException
			.Should().BeOfType<Npgsql.PostgresException>().Which.SqlState.Should().Be(Npgsql.PostgresErrorCodes.ForeignKeyViolation);
		(await Context.Products.AsNoTracking().AnyAsync(item => item.Id == Product.Id, CancellationToken)).Should().BeFalse();
		(await Context.ProductContents.AsNoTracking().AnyAsync(item => item.ChildProductId == Product.Id, CancellationToken)).Should().BeFalse();
	}

	private async Task AddBlocker(ProductDeletionBlocker blocker)
	{
		switch (blocker)
		{
			case ProductDeletionBlocker.Stock:
				Product.IncreaseStock(1);
				break;
			case ProductDeletionBlocker.StorageContent:
				await new StorageContentBuilder(Faker).WithStorageCode(StorageCode).WithCurrencyId(CurrencyId)
					.WithProductIds(Product.Id).WithCount(0).BuildAndAddToDb(Context, CancellationToken);
				break;
			case ProductDeletionBlocker.Reservation:
				await new ProductReservationBuilder(Faker).WithOrganizationId(UserId).WithProductId(Product.Id)
					.WithReservedCount(3).WithCurrentCount(3).BuildAndAddToDb(Context, CancellationToken);
				break;
			case ProductDeletionBlocker.Composition:
				await Context.AddAsync(ProductContent.Create(Other.Id, Product.Id, 1), CancellationToken);
				break;
			case ProductDeletionBlocker.Order:
				var order = Order.CreateManual(UserId, UserId, CurrencyId);
				order.AddItem(OrderItem.Create(order, Product.Id, 1));
				await Context.AddAsync(order, CancellationToken);
				break;
			case ProductDeletionBlocker.Sale:
			case ProductDeletionBlocker.Purchase:
				// The storage record belongs to a different product: only the document blocks the target.
				var storage = await new StorageContentBuilder(Faker).WithStorageCode(StorageCode).WithCurrencyId(CurrencyId)
					.WithProductIds(Other.Id).WithCount(0).BuildAndAddToDb(Context, CancellationToken);
				var receiver = await new OrganizationBuilder(Faker).WithOwnerId(UserId).BuildAndAddToDb(Context, CancellationToken);
				var transaction = await new TransactionBuilder(Faker).WithSenderId(UserId).WithReceiverId(receiver.Id)
					.WithCurrencyId(CurrencyId).WithAmount(10m).BuildAndAddToDb(Context, CancellationToken);
				if (blocker == ProductDeletionBlocker.Sale)
				{
					var content = new SaleContentBuilder(Faker).WithProductId(Product.Id).WithCurrencyId(CurrencyId)
						.WithStorageContentIds([storage.Id]).WithCount(1).WithDetailsCount(1).Build();
					var sale = new SaleBuilder(Faker).WithUserId(UserId).WithOrganizationId(UserId).WithCurrencyId(CurrencyId)
						.WithStorageCode(StorageCode).WithTransactionId(transaction.Id).WithContents([content]).Completed().Build();
					sale.Delete();
					await Context.AddAsync(sale, CancellationToken);
				}
				else
					await new PurchaseBuilder(Faker).WithSupplierUserId(UserId).WithSupplierOrganizationId(UserId)
						.WithCurrencyId(CurrencyId).WithStorage(StorageCode).WithTransactionId(transaction.Id)
						.WithContent(new PurchaseContentBuilder(Faker).WithProductId(Product.Id).WithStorageContentId(storage.Id).Build())
						.BuildAndAddToDb(Context, CancellationToken);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(blocker));
		}
		await Context.SaveChangesAsync(CancellationToken);
	}

	private Task<Unit> Delete() => Mediator.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), CancellationToken);
}
