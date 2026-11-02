using Enums;
using FluentAssertions;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;
using Main.Application.Static;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class CreateProducerSupplierMappingTests : IntegrationTest
{
	public CreateProducerSupplierMappingTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	public async Task CreateProducerSupplierMapping_InvalidSupplierProducerName_ThrowsValidationException(
		string supplierProducerName)
	{
		var command = new CreateProducerSupplierMappingCommand(
			new NewProducerSupplierMapping
			{
				ProducerId = TestContext.Producers[0].Id,
				Supplier = Supplier.Armtek,
				SupplierProducerName = supplierProducerName
			});

		await Assert.ThrowsAsync<ValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task CreateProducerSupplierMapping_InvalidProducerId_ThrowsProducerNotFound()
	{
		var command = new CreateProducerSupplierMappingCommand(
			new NewProducerSupplierMapping
			{
				ProducerId = int.MaxValue,
				Supplier = Supplier.Armtek,
				SupplierProducerName = Faker.Lorem.Letter(20)
			});

		var exception = await Assert.ThrowsAsync<DbValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));

		Assert.IsType<ProducerNotFoundException>(exception.Failures[0].Error);
	}

	[Theory]
	[InlineData(" Bosch ")]
	[InlineData("bosch")]
	[InlineData("BOSCH")]
	public async Task
		CreateProducerSupplierMapping_WithSameSupplierNameAndSupplier_ThrowsMappingAlreadyExists(string duplicateName)
	{
		const string supplierProducerName = "Bosch";
		var existing = await new ProducerSupplierMappingBuilder(Faker)
			.WithProducerId(TestContext.Producers[0].Id)
			.WithSupplier(Supplier.Armtek)
			.WithSupplierProducerName($"  {supplierProducerName}  ")
			.BuildAndAddToDb(Context, CancellationToken);

		var command = new CreateProducerSupplierMappingCommand(
			new NewProducerSupplierMapping
			{
				ProducerId = TestContext.Producers[1].Id,
				Supplier = existing.Supplier,
				SupplierProducerName = duplicateName
			});

		var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));
		exception.Failures.Should().ContainSingle(failure => failure.Error is ProducersSupplierMappingAlreadyExistsException);
	}

	[Fact]
	public async Task CreateProducerSupplierMapping_WithSameProducerAndSupplierButDifferentName_Succeeds()
	{
		var producer = TestContext.Producers[0];
		await new ProducerSupplierMappingBuilder(Faker)
			.WithProducerId(producer.Id)
			.WithSupplier(Supplier.Armtek)
			.WithSupplierProducerName("Bosch")
			.BuildAndAddToDb(Context, CancellationToken);
		var command = new CreateProducerSupplierMappingCommand(
			new NewProducerSupplierMapping
			{
				ProducerId = producer.Id,
				Supplier = Supplier.Armtek,
				SupplierProducerName = "Bosch Automotive"
			});

		await Mediator.Send(command, CancellationToken);

		var mappings = await Context
			.ProducerSupplierMappings
			.AsNoTracking()
			.Where(x => x.ProducerId == producer.Id)
			.ToListAsync(CancellationToken);
		mappings.Should().HaveCount(2);
	}

	[Fact]
	public async Task SameNameForAnotherSupplier_IsAllowed()
	{
		var producerId = TestContext.Producers[0].Id;
		await new ProducerSupplierMappingBuilder(Faker)
			.WithProducerId(producerId).WithSupplier(Supplier.Armtek)
			.WithSupplierProducerName("Bosch").BuildAndAddToDb(Context, CancellationToken);
		var otherSupplier = Enum.GetValues<Supplier>().First(supplier => supplier != Supplier.Armtek);

		await Mediator.Send(new CreateProducerSupplierMappingCommand(new NewProducerSupplierMapping
		{
			ProducerId = producerId, Supplier = otherSupplier, SupplierProducerName = " bosch "
		}), CancellationToken);

		(await Context.ProducerSupplierMappings.CountAsync(CancellationToken)).Should().Be(2);
	}

	[Fact]
	public async Task CreateProducerSupplierMapping_Normal_Succeeds()
	{
		var producer = TestContext.Producers[0];
		var supplierProducerName = $"  {Faker.Lorem.Letter(20)}  ";
		var command = new CreateProducerSupplierMappingCommand(
			new NewProducerSupplierMapping
			{
				ProducerId = producer.Id,
				Supplier = Supplier.Armtek,
				SupplierProducerName = supplierProducerName
			});

		var result = await Mediator.Send(command, CancellationToken);

		result.ProducerSupplierMapping.ProducerId.Should().Be(producer.Id);
		result.ProducerSupplierMapping.Supplier.Should().Be(command.ProducerSupplierMapping.Supplier);
		result.ProducerSupplierMapping.SupplierProducerName.Should().Be(supplierProducerName.Trim());

		var mapping = await Context.ProducerSupplierMappings.AsNoTracking().SingleAsync(CancellationToken);

		mapping.Id.Should().Be(result.ProducerSupplierMapping.Id);
		mapping.ProducerId.Should().Be(producer.Id);
		mapping.Supplier.Should().Be(command.ProducerSupplierMapping.Supplier);
		mapping.Name.Should().Be(supplierProducerName.Trim());
		mapping.NormalizedName.Should().Be(supplierProducerName.Trim().ToUpperInvariant());
	}
}
