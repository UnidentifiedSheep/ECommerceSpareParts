using System.Globalization;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.ProductCharacteristicsImport;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductCharacteristicsImportLrtStepTests : CsvLrtIntegrationTest<ProductCharacteristicsImportLrtStep>
{
	public ProductCharacteristicsImportLrtStepTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task ImportsByNormalizedName_UpdatesExistingPairsAndCanBeRepeated()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var color = Characteristic.Create("Brake Color");
		var size = Characteristic.Create("Размер детали");
		var material = Characteristic.Create("Material");
		Context.Characteristics.AddRange(color, size, material);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.AddRange(
			ProductCharacteristic.Create(products[0].Id, color.Id, "Old"),
			ProductCharacteristic.Create(products[0].Id, material.Id, "Steel"));
		await Context.SaveChangesAsync(CancellationToken);
		string[] rows =
		[
			CsvRow(products[0].Id, " BRAKE COLOR ", " Red "),
			CsvRow(products[1].Id, "brake-color", "Blue"),
			CsvRow(products[0].Id, "razmer-detali", "100 mm"),
			CsvRow(products[1].Id, "Размер детали", " 200 mm "),
			CsvRow(products[0].Id, "brake-color", "Green"),
			CsvRow(products[1].Id, "РАЗМЕР ДЕТАЛИ", "300 mm"),
			CsvRow(products[0].Id, "Brake Color", "x")
		];

		for (var attempt = 0; attempt < 2; attempt++)
		{
			var execution = await ExecuteCsv("ProductId,Characteristic,Value", rows,
				fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

			execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
			var state = execution.GetState<ProductCharacteristicsImportState>();
			state.Checkpoint!.CurrentLine.Should().Be(7);
			state.Errors.Should().ContainSingle(error => error.RowIdx == 7);
		}

		var values = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.ToDictionary(x => x.GetId(), x => x.Value);
		values.Should().HaveCount(5);
		values[(products[0].Id, color.Id)].Should().Be("Green");
		values[(products[1].Id, color.Id)].Should().Be("Blue");
		values[(products[0].Id, size.Id)].Should().Be("100 mm");
		values[(products[1].Id, size.Id)].Should().Be("300 mm");
		values[(products[0].Id, material.Id)].Should().Be("Steel");
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(3);
	}

	[Fact]
	public async Task InvalidRows_AreReportedWithoutDiscardingValidValues()
	{
		var productId = TestContext.Products[0].Id;
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			[
				CsvRow(productId, "Unknown characteristic", "Red"),
				CsvRow(productId, "", "Red"),
				CsvRow("invalid", "Color", "Red"),
				CsvRow(productId, "Color", ""),
				CsvRow(productId, "Color", "ab"),
				CsvRow(productId, "Color", new string('x', 129)),
				CsvRow(productId, "Color", "  Valid  ")
			],
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductCharacteristicsImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(7);
		state.Errors.Select(error => error.RowIdx).Should().Equal(Enumerable.Range(1, 6));
		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(productId);
		value.CharacteristicId.Should().Be(characteristic.Id);
		value.Value.Should().Be("Valid");
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task MissingProduct_FailsTheJobAndRollsBackTheWholeBatch()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.Add(ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Old"));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			[
				CsvRow(products[0].Id, "Color", "Updated"),
				CsvRow(int.MaxValue, "Color", "Missing"),
				CsvRow(products[1].Id, "Color", "New")
			],
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.Attempts.Should().Be(1);
		execution.Job.ErrorMessage.Should().Contain("product_characteristics_product_id_fk");
		execution.GetState<ProductCharacteristicsImportState>().Checkpoint.Should().BeNull();
		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(products[0].Id);
		value.CharacteristicId.Should().Be(characteristic.Id);
		value.Value.Should().Be("Old");
	}

	[Fact]
	public async Task BulkUpsert_RollsBackInsertsAndUpdatesWithTheTransaction()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.Add(ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Old"));
		await Context.SaveChangesAsync(CancellationToken);
		var repository = Scope.ServiceProvider.GetRequiredService<IProductRepository>();

		await using (var transaction = await Context.Database.BeginTransactionAsync(CancellationToken))
		{
			await repository.UpsertProductCharacteristicsAsync(
			[
				ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Updated"),
				ProductCharacteristic.Create(products[1].Id, characteristic.Id, "New")
			], CancellationToken);
			await transaction.RollbackAsync(CancellationToken);
		}

		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(products[0].Id);
		value.CharacteristicId.Should().Be(characteristic.Id);
		value.Value.Should().Be("Old");
	}

	[Fact]
	public async Task RepeatedPairAcrossBatches_UpdatesThePersistedValueAndCheckpoint()
	{
		var productId = TestContext.Products[0].Id;
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			Enumerable.Range(1, 1001).Select(value => CsvRow(productId, "COLOR", value.ToString("D4", CultureInfo.InvariantCulture))),
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductCharacteristicsImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(1001);
		state.Errors.Should().BeEmpty();
		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(productId);
		value.CharacteristicId.Should().Be(characteristic.Id);
		value.Value.Should().Be("1001");
	}
}
