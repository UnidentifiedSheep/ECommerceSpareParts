using Application.Common.Models;
using Enums;
using FluentAssertions;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Queries.ProducerSupplierMappings.GetProducerSupplierMappings;
using Tests.Abstractions.Test;
using Tests.DataBuilders;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class GetProducerSupplierMappingsTests(
	ReadScenarioFixture<GetProducerSupplierMappingsTests.ProducerSupplierMappingsScenario> fixture)
	: ReadScenarioTest<GetProducerSupplierMappingsTests.ProducerSupplierMappingsScenario>(fixture)
{
	[Fact]
	public async Task GetProducerSupplierMappings_ByProducer_ReturnsOnlyProducerMappings()
	{
		var expected = Scenario.OtherProducerMapping;

		var result = await Mediator.Send(
			new GetProducerSupplierMappingsQuery(
				expected.ProducerId,
				[],
				new Pagination(0, 10)),
			CancellationToken);

		result.Mappings.Should().ContainSingle();
		result.Mappings[0].Id.Should().Be(expected.Id);
		result.Mappings[0].ProducerId.Should().Be(expected.ProducerId);
		result.Mappings[0].Supplier.Should().Be(expected.Supplier);
		result.Mappings[0].SupplierProducerName.Should().Be(expected.Name);
	}

	[Fact]
	public async Task GetProducerSupplierMappings_BySupplier_ReturnsOnlyRequestedSuppliers()
	{
		var expected = Scenario.ArmtekMapping;

		var result = await Mediator.Send(
			new GetProducerSupplierMappingsQuery(
				expected.ProducerId,
				[Supplier.Armtek],
				new Pagination(0, 10)),
			CancellationToken);

		result.Mappings.Should().ContainSingle();
		result.Mappings[0].Id.Should().Be(expected.Id);
		result.Mappings[0].Supplier.Should().Be(Supplier.Armtek);
	}

	[Fact]
	public async Task GetProducerSupplierMappings_WithPagination_ReturnsRequestedPage()
	{
		var result = await Mediator.Send(
			new GetProducerSupplierMappingsQuery(
				Scenario.ArmtekMapping.ProducerId,
				[],
				new Pagination(0, 1)),
			CancellationToken);

		result.Mappings.Should().ContainSingle();
	}

	public sealed class ProducerSupplierMappingsScenario : ReadScenario
	{
		public ProducerSupplierMappingsScenario() => RegisterBasicContext<ProducerTestContext>();

		public ProducerSupplierMapping ArmtekMapping { get; private set; } = null!;

		public ProducerSupplierMapping OtherProducerMapping { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			var producers = Get<ProducerTestContext>().Producers;
			ArmtekMapping = new ProducerSupplierMappingBuilder(Faker)
				.WithProducerId(producers[0].Id)
				.WithSupplier(Supplier.Armtek)
				.Build();
			var favoritMapping = new ProducerSupplierMappingBuilder(Faker)
				.WithProducerId(producers[0].Id)
				.WithSupplier(Supplier.FavoritParts)
				.Build();
			OtherProducerMapping = new ProducerSupplierMappingBuilder(Faker)
				.WithProducerId(producers[1].Id)
				.WithSupplier(Supplier.Armtek)
				.WithSupplierProducerName("Armtek producer")
				.Build();
			return [ArmtekMapping, favoritMapping, OtherProducerMapping];
		}
	}
}

public class GetProducerSupplierMappingsValidationTests : ValidationTest<GetProducerSupplierMappingsValidation
	, GetProducerSupplierMappingsQuery>
{
	[Fact]
	public async Task GetProducerSupplierMappings_InvalidPagination_FailsValidation()
	{
		var query = new GetProducerSupplierMappingsQuery(
			1,
			[],
			new Pagination(-1, 10));

		var result = await ValidateAsync(query);
		result.ShouldHaveValidationErrorFor("Pagination.Page");
	}
}
