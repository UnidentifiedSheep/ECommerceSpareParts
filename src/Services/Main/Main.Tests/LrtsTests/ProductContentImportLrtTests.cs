using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.ProductContentImport;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductContentImportLrtTests : CsvLrtIntegrationTest<ProductContentImportLrt>
{
	public ProductContentImportLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task ImportsDirectedPairs_UpdatesQuantityAndCanBeRepeated()
	{
		var products = TestContext.Products.Take(3).ToArray();
		Context.ProductContents.AddRange(
			ProductContent.Create(products[0].Id, products[1].Id, 1),
			ProductContent.Create(products[0].Id, products[2].Id, 11));
		await Context.SaveChangesAsync(CancellationToken);
		string[] rows =
		[
			CsvRow(products[0].Id, products[1].Id, 2),
			CsvRow(products[1].Id, products[0].Id, 4),
			CsvRow(products[0].Id, products[1].Id, 5),
			CsvRow(products[2].Id, products[1].Id, 0)
		];

		for (var attempt = 0; attempt < 2; attempt++)
		{
			var execution = await ExecuteCsv("ParentId,ChildId,Quantity", rows,
				fileName => new ProductContentImportInputState { FileName = fileName });

			execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
			var state = execution.GetState<ProductContentImportState>();
			state.Checkpoint!.CurrentLine.Should().Be(4);
			state.Errors.Should().BeEmpty();
		}

		var quantities = (await Context.ProductContents.AsNoTracking().ToListAsync(CancellationToken))
			.ToDictionary(x => x.GetId(), x => x.Quantity);
		quantities.Should().HaveCount(4);
		quantities[(products[0].Id, products[1].Id)].Should().Be(5);
		quantities[(products[1].Id, products[0].Id)].Should().Be(4);
		quantities[(products[0].Id, products[2].Id)].Should().Be(11);
		quantities[(products[2].Id, products[1].Id)].Should().Be(0);
	}

	[Fact]
	public async Task InvalidRows_AreReportedWithoutDiscardingValidContents()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var execution = await ExecuteCsv(
			"ParentId,ChildId,Quantity",
			[
				CsvRow(0, products[1].Id, 1),
				CsvRow(products[0].Id, -1, 1),
				CsvRow(products[0].Id, products[0].Id, 1),
				CsvRow(products[0].Id, products[1].Id, -1),
				CsvRow("not-an-id", products[1].Id, 1),
				CsvRow(products[0].Id, products[1].Id, "not-a-quantity"),
				CsvRow(products[0].Id, products[1].Id, 3)
			],
			fileName => new ProductContentImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductContentImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(7);
		state.Errors.Select(error => error.RowIdx).Should().Equal(Enumerable.Range(1, 6));
		var content = (await Context.ProductContents.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		content.GetId().Should().Be((products[0].Id, products[1].Id));
		content.Quantity.Should().Be(3);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task MissingParentOrChild_FailsTheJobAndRollsBackTheWholeBatch(bool missingParent)
	{
		var products = TestContext.Products.Take(3).ToArray();
		Context.ProductContents.Add(ProductContent.Create(products[0].Id, products[1].Id, 2));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ParentId,ChildId,Quantity",
			[
				CsvRow(products[0].Id, products[1].Id, 8),
				CsvRow(missingParent ? int.MaxValue : products[0].Id,
					missingParent ? products[1].Id : int.MaxValue, 1),
				CsvRow(products[1].Id, products[2].Id, 7)
			],
			fileName => new ProductContentImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.Attempts.Should().Be(1);
		execution.Job.ErrorMessage.Should().Contain(
			missingParent ? "product_contents_parent_fk" : "product_contents_child_fk");
		execution.GetState<ProductContentImportState>().Checkpoint.Should().BeNull();
		var content = (await Context.ProductContents.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		content.GetId().Should().Be((products[0].Id, products[1].Id));
		content.Quantity.Should().Be(2);
	}

	[Fact]
	public async Task BulkUpsert_RollsBackInsertsAndUpdatesWithTheTransaction()
	{
		var products = TestContext.Products.Take(3).ToArray();
		Context.ProductContents.Add(ProductContent.Create(products[0].Id, products[1].Id, 2));
		await Context.SaveChangesAsync(CancellationToken);
		var repository = Scope.ServiceProvider.GetRequiredService<IProductRepository>();

		await using (var transaction = await Context.Database.BeginTransactionAsync(CancellationToken))
		{
			await repository.UpsertProductContentsAsync(
			[
				ProductContent.Create(products[0].Id, products[1].Id, 8),
				ProductContent.Create(products[0].Id, products[2].Id, 7)
			], CancellationToken);
			await transaction.RollbackAsync(CancellationToken);
		}

		var content = (await Context.ProductContents.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		content.GetId().Should().Be((products[0].Id, products[1].Id));
		content.Quantity.Should().Be(2);
	}

	[Fact]
	public async Task RepeatedPairAcrossBatches_UpdatesQuantityAndCheckpoint()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var execution = await ExecuteCsv(
			"ParentId,ChildId,Quantity",
			Enumerable.Range(1, 1001).Select(quantity => CsvRow(products[0].Id, products[1].Id, quantity)),
			fileName => new ProductContentImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductContentImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(1001);
		state.Errors.Should().BeEmpty();
		var content = (await Context.ProductContents.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		content.GetId().Should().Be((products[0].Id, products[1].Id));
		content.Quantity.Should().Be(1001);
	}
}
