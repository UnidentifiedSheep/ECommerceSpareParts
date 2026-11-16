using System.Linq.Expressions;
using System.Text.Json;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Contracts.ProductEnrichment;
using Contracts.Products;
using Domain.CommonEntities.Job;
using Exceptions;
using FluentAssertions;
using Main.Entities.Product;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using Main.Entities.Product.Product.Enrichment;
using Main.Entities.Product.Stock;
using Main.Modules.Product.Application.Lrts.DeleteProductImages;
using Main.Modules.Product.Application.Products.Commands;
using Main.Modules.Product.Application.Products.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Persistence.Context;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Products;

public class DeleteProductTests : IntegrationTest
{
	public DeleteProductTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private Product Product => GetContext<ProductTestContext>().Products[0];
	private Product Other => GetContext<ProductTestContext>().Products[1];
	private Guid UserId => GetContext<UserContextTestContext>().SystemUser.Id;
	private string StorageCode => GetContext<StorageTestContext>().Storages.First().Code;
	private S3StorageServiceStub S3 => Sp.GetRequiredService<S3StorageServiceStub>();

	private MessageBrokerStub Publisher
		=> (MessageBrokerStub)Scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

	[Fact]
	public async Task UnusedProduct_DeletesDependentsAndQueuesImagesWithoutDeletingFiles()
	{
		var characteristic =
			await new CharacteristicBuilder(Faker).BuildAndAddToDb(Context, CancellationToken);
		var image = ProductImage.Create(Product.Id, ".jpg");
		var secondImage = ProductImage.Create(Product.Id, ".png");
		var otherImage = ProductImage.Create(Other.Id, ".jpg");
		Product.AddEan("1234567890123");
		await Context.AddRangeAsync(
			[
				image, secondImage, otherImage,
				ProductCharacteristic.Create(
					Product.Id,
					characteristic.Id,
					"value"),
				new ProductSizeBuilder(Faker).WithProductId(Product.Id).Build(),
				new ProductWeightBuilder(Faker).WithProductId(Product.Id).Build(),
				ProductContent.Create(
					Product.Id,
					Other.Id,
					2),
				Main.Entities.Cart.Cart.Cart.Create(
					UserId,
					Product.Id,
					1)
			],
			CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		S3.SetFile(
			"images",
			image.StorageKey,
			"image");
		S3.SetFile(
			"images",
			secondImage.StorageKey,
			"second image");

		var id = Product.Id;
		await DeleteInNewScope(id, Product.RowVersion);

		(await Exists<Product>(item => item.Id == id)).Should().BeFalse();
		(await Exists<ProductEan>(item => item.ProductId == id)).Should().BeFalse();
		(await Exists<ProductCharacteristic>(item => item.ProductId == id)).Should().BeFalse();
		(await Exists<ProductSize>(item => item.ProductId == id)).Should().BeFalse();
		(await Exists<ProductWeight>(item => item.ProductId == id)).Should().BeFalse();
		(await Exists<ProductContent>(item => item.ParentProductId == id)).Should().BeFalse();
		(await Exists<Main.Entities.Cart.Cart.Cart>(item => item.ProductId == id)).Should().BeFalse();
		(await Context.Set<ProductImage>().AsNoTracking().ToListAsync(CancellationToken)).Should()
			.ContainSingle()
			.Which
			.StorageKey
			.Should()
			.Be(otherImage.StorageKey);
		(await Exists<Product>(item => item.Id == Other.Id)).Should().BeTrue();
		(await Exists<Characteristic>(item => item.Id == characteristic.Id)).Should().BeTrue();
		var job = (await Context.Set<Job>()
				.AsNoTracking()
				.Where(item => item.SystemName == DeleteProductImagesLrt.Name)
				.ToListAsync(CancellationToken))
			.Should()
			.ContainSingle("deletion must persist a cleanup job with the image keys")
			.Which;
		var input = JsonSerializer.Deserialize<DeleteProductImagesInputState>(job.State)!;
		input.ProductId.Should().Be(id);
		input.BucketName.Should().Be("images");
		input.StorageKeys
			.Should()
			.BeEquivalentTo(
				image.StorageKey,
				secondImage.StorageKey);
		(await S3.DownloadFileAsync(
				"images",
				image.StorageKey,
				CancellationToken)).IsSuccess
			.Should()
			.BeTrue();
		(await S3.DownloadFileAsync(
				"images",
				secondImage.StorageKey,
				CancellationToken)).IsSuccess
			.Should()
			.BeTrue();
	}

	[Fact]
	public async Task Delete_InvalidatesProductAndCrossCaches()
	{
		await Context.AddAsync(ProductCross.Create(Product.Id, Other.Id), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		await provider.GetProductOrSetAsync(Product.Id, CancellationToken);
		await provider.GetProductOrSetAsync(Other.Id, CancellationToken);
		(await provider.GetProductCrossesAsync(Other.Id, CancellationToken)).Should().Contain(Product.Id);

		await Delete();

		(await provider.GetProductAsync(Product.Id, CancellationToken)).Should().BeNull();
		(await provider.GetProductAsync(Other.Id, CancellationToken)).Should().NotBeNull();
		(await provider.GetProductCrossesAsync(Other.Id, CancellationToken)).Should().BeEmpty();
		(await Context.Set<ProductCross>().AsNoTracking().ToListAsync(CancellationToken)).Should().BeEmpty();
		(await Exists<Job>(item => item.SystemName == DeleteProductImagesLrt.Name)).Should().BeFalse();
	}

	[Fact]
	public async Task Delete_PublishesTombstoneForSearchConsumers()
	{
		var publishedBefore = Publisher.PublishedMessages.Count;

		await Delete();

		Publisher.PublishedMessages
			.Skip(publishedBefore)
			.OfType<ProductsUpdatedEvent>()
			.SelectMany(item => item.Products)
			.Should()
			.ContainSingle(item => item.Id == Product.Id && item.IsDeleted);
	}

	[Fact]
	public async Task Delete_DetachesPairAndCatalogueCandidateAndPublishesMappingChange()
	{
		Other.SetPair(Product.Id);
		var candidate = await new CatalogueCandidateBuilder(Faker).WithProductId(Product.Id)
			.WithProducerId(Product.ProducerId)
			.BuildAndAddToDb(Context, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();
		var publishedBefore = Publisher.PublishedMessages.Count;

		await DeleteInNewScope(Product.Id, Product.RowVersion);

		(await Context.Set<Product>()
				.AsNoTracking()
				.SingleAsync(item => item.Id == Other.Id, CancellationToken)).PairId
			.Should()
			.BeNull();
		(await Context.Set<CatalogueCandidate>()
				.AsNoTracking()
				.SingleAsync(item => item.Id == candidate.Id, CancellationToken)).ProductId
			.Should()
			.BeNull();
		Publisher.PublishedMessages
			.Skip(publishedBefore)
			.OfType<CatalogueCandidateUpdatedEvent>()
			.Should()
			.ContainSingle(item
				=> item.Candidate.Id == candidate.Id && item.Candidate.MappedProductId == null);
	}

	[Fact]
	public async Task Delete_WithTrackedPair_DetachesPairWithoutConcurrencyConflict()
	{
		Other.SetPair(Product.Id);
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();

		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		await transaction.WithSettings(TransactionalAttribute.Serializable(30, 0))
			.ExecuteAsync(
				async (_, ct) => await Mediator.Send(
					new DeleteProductCommand(Product.Id, Product.RowVersion),
					ct),
				CancellationToken);

		(await Exists<Product>(item => item.Id == Product.Id)).Should().BeFalse();
		(await Context.Set<Product>()
				.AsNoTracking()
				.SingleAsync(item => item.Id == Other.Id, CancellationToken))
			.PairId
			.Should()
			.BeNull();
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
		(await Context.Set<Product>()
				.AsNoTracking()
				.SingleAsync(item => item.Id == Product.Id, CancellationToken))
			.Description
			.Should()
			.Be("updated since opening the deletion dialog");
	}

	[Fact]
	public async Task ReferencedProduct_IsPreservedWithoutEventsOrCleanup()
	{
		await Context.AddAsync(
			ProductStock.Create(
				StorageCode,
				Product.Id,
				1),
			CancellationToken);
		var image = ProductImage.Create(Product.Id, ".jpg");
		await Context.AddAsync(image, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		await provider.GetProductOrSetAsync(Product.Id, CancellationToken);
		var publishedBefore = Publisher.PublishedMessages.Count;

		var action = () => Delete();

		(await action.Should().ThrowAsync<ProductDeletionNotAllowedException>()).Which
			.LocalizableMessage
			.MessageKey
			.Should()
			.Be(ArticleDeletionStockMessage.Key);
		(await Exists<Product>(item => item.Id == Product.Id)).Should().BeTrue();
		(await Exists<ProductImage>(item => item.ProductId == Product.Id)).Should().BeTrue();
		(await provider.GetProductAsync(Product.Id, CancellationToken)).Should().NotBeNull();
		(await Exists<Job>(item => item.SystemName == DeleteProductImagesLrt.Name)).Should().BeFalse();
		Publisher.PublishedMessages.Skip(publishedBefore).Should().BeEmpty();
	}

	[Fact]
	public async Task OuterTransactionRollback_PreservesProductAndDoesNotQueueCleanupOrPublishDeletion()
	{
		var image = ProductImage.Create(Product.Id, ".jpg");
		await Context.AddAsync(image, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		DiscardPendingDomainEvents();
		S3.SetFile(
			"images",
			image.StorageKey,
			"image");
		var publishedBefore = Publisher.PublishedMessages.Count;
		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var action = () => transaction.WithSettings(TransactionalAttribute.Serializable(30, 1))
			.ExecuteAsync(
				async (_, ct) =>
				{
					await Mediator.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), ct);
					throw new InvalidOperationException("Rollback test");
				},
				CancellationToken);

		await action.Should().ThrowAsync<InvalidOperationException>();

		using var verification = Sp.CreateScope();
		var db = verification.ServiceProvider.GetRequiredService<UnifiedDbContext>();
		(await db.Set<Product>().AnyAsync(item => item.Id == Product.Id, CancellationToken)).Should()
			.BeTrue();
		(await db.Set<ProductImage>().AnyAsync(item => item.ProductId == Product.Id, CancellationToken))
			.Should()
			.BeTrue();
		(await db.Set<Job>()
				.AnyAsync(item => item.SystemName == DeleteProductImagesLrt.Name, CancellationToken)).Should()
			.BeFalse();
		Publisher.PublishedMessages
			.Skip(publishedBefore)
			.OfType<ProductsUpdatedEvent>()
			.SelectMany(item => item.Products)
			.Should()
			.NotContain(item => item.Id == Product.Id && item.IsDeleted);
		(await S3.DownloadFileAsync(
				"images",
				image.StorageKey,
				CancellationToken)).IsSuccess
			.Should()
			.BeTrue();
	}

	[Fact]
	public async Task ConcurrentCompositionInsert_CannotAttachToDeletedProduct()
	{
		using var insertionScope = Sp.CreateScope();
		var insertionDb = insertionScope.ServiceProvider.GetRequiredService<UnifiedDbContext>();
		var content = ProductContent.Create(
			Other.Id,
			Product.Id,
			1);
		insertionDb.Set<ProductContent>().Add(content);
		Task<int>? insertion = null;
		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();

		await transaction.WithSettings(TransactionalAttribute.Serializable(30, 1))
			.ExecuteAsync(
				async (_, ct) =>
				{
					await Mediator.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), ct);
					insertion = insertionDb.SaveChangesAsync(ct);
					insertion.IsCompleted.Should().BeFalse();
				},
				CancellationToken);

		var action = async () => await insertion!;
		(await action.Should().ThrowAsync<DbUpdateException>()).Which
			.InnerException
			.Should()
			.BeOfType<PostgresException>()
			.Which
			.SqlState
			.Should()
			.Be(PostgresErrorCodes.ForeignKeyViolation);
		(await Exists<Product>(item => item.Id == Product.Id)).Should().BeFalse();
		(await Exists<ProductContent>(item => item.ChildProductId == Product.Id)).Should().BeFalse();
	}

	private Task<bool> Exists<T>(Expression<Func<T, bool>> predicate) where T : class
		=> Context.Set<T>().AsNoTracking().AnyAsync(predicate, CancellationToken);

	private Task<Unit> Delete()
		=> Mediator.Send(new DeleteProductCommand(Product.Id, Product.RowVersion), CancellationToken);

	private async Task DeleteInNewScope(int id, uint rowVersion)
	{
		using var deletionScope = Sp.CreateScope();
		await deletionScope.ServiceProvider
			.GetRequiredService<IMediator>()
			.Send(new DeleteProductCommand(id, rowVersion), CancellationToken);
	}
}
