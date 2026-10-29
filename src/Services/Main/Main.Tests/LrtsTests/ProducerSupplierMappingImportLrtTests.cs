using System.Text.Json;
using Contracts.Producer;
using Domain.CommonEnums;
using Enums;
using FluentAssertions;
using Main.Application.Lrts.ProducerSupplierMappingImport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Test;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class
	ProducerSupplierMappingImportLrtTests : LrtIntegrationTest<ProducerSupplierMappingImportLrt>
{
	private const string UploadsBucket = "uploads";

	public ProducerSupplierMappingImportLrtTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

	[Fact]
	public async Task DifferentNamesForSameProducerAndSupplier_AreInserted()
	{
		var producer = TestContext.Producers[0];

		var execution = await ExecuteCsv(
			CsvRow(
				producer.Name,
				Supplier.Armtek,
				"Bosch"),
			CsvRow(
				producer.Name,
				Supplier.Armtek,
				"Bosch Automotive"));

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var mappings = await Context
			.ProducerSupplierMappings
			.AsNoTracking()
			.Where(x => x.ProducerId == producer.Id)
			.ToListAsync(CancellationToken);
		mappings.Should().HaveCount(2);
		((MessageBrokerStub)Scope.ServiceProvider.GetRequiredService<PublishEndpointAccessor>().Publisher)
			.PublishedMessagesOfType<ProducerLookupChangedEvent>().Should().NotBeEmpty();
	}

	[Fact]
	public async Task ExistingSupplierName_IsNotRemappedToAnotherProducer()
	{
		var existing = await new ProducerSupplierMappingBuilder(Faker)
			.WithProducerId(TestContext.Producers[0].Id)
			.WithSupplier(Supplier.Armtek)
			.WithSupplierProducerName("Bosch")
			.BuildAndAddToDb(Context, CancellationToken);

		var execution = await ExecuteCsv(
			CsvRow(
				TestContext.Producers[1].Name,
				existing.Supplier,
				$" {existing.Name.ToUpperInvariant()} "));

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		Context.ChangeTracker.Clear();
		var persisted = await Context.ProducerSupplierMappings.AsNoTracking().SingleAsync(CancellationToken);
		persisted.ProducerId.Should().Be(existing.ProducerId);
	}

	[Fact]
	public async Task InvalidAndDuplicateRows_AreReported_AndValidMappingIsTrimmed()
	{
		var producer = TestContext.Producers[0];

		var execution = await ExecuteCsv(
			CsvRow(
				producer.Name,
				Supplier.Armtek,
				" Bosch "),
			CsvRow(
				producer.Name,
				Supplier.Armtek,
				"bOsCh"),
			CsvRow(
				"Unknown producer",
				Supplier.Armtek,
				"Unknown"),
			CsvRow(
				producer.Name,
				Supplier.Armtek,
				" "));

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProducerSupplierMappingImportState>();
		state.Errors.Should().HaveCount(3);

		var mapping = await Context.ProducerSupplierMappings.AsNoTracking().SingleAsync(CancellationToken);
		mapping.Name.Should().Be("Bosch");
		mapping.NormalizedName.Should().Be("BOSCH");
	}

	private async Task<LrtExecutionResult> ExecuteCsv(params string[] rows)
	{
		var fileName = $"{Guid.NewGuid():N}.csv";
		var csv = string.Join(
			Environment.NewLine,
			new[]
			{
				"Producer,Supplier,SupplierProducer"
			}.Concat(rows));
		Scope
		.ServiceProvider
		.GetRequiredService<S3StorageServiceStub>()
		.SetFile(
			UploadsBucket,
			fileName,
			csv);

		return await ExecuteLrt(
			JsonSerializer.Serialize(
				new ProducerSupplierMappingImportInputState
				{
					FileName = fileName
				}));
	}

	private static string CsvRow(
		string producer,
		Supplier supplier,
		string supplierProducerName) => $"{Escape(producer)},{supplier},{Escape(supplierProducerName)}";

	private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
