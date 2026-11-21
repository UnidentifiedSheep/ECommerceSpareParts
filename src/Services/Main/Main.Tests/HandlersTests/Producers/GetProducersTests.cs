using Application.Common.Models;
using FluentAssertions;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Queries.Producers.GetProducers;
using Tests.DataBuilders;

namespace Tests.HandlersTests.Producers;

public class GetProducersTests(ReadScenarioFixture<GetProducersTests.ProducersScenario> fixture)
	: ReadScenarioTest<GetProducersTests.ProducersScenario>(fixture)
{
	[Fact]
	public async Task GetProducers_WithMultipleIds_ReturnsOnlyRequestedProducers()
	{
		var result = await Mediator.Send(
			CreateQuery([Scenario.AlphaRequested.Id, Scenario.BetaRequested.Id]),
			CancellationToken);

		result.Producers.Select(x => x.Id).Should().BeEquivalentTo([Scenario.AlphaRequested.Id, Scenario.BetaRequested.Id]);
		result.Producers.Should().NotContain(x => x.Id == Scenario.Gamma.Id);
	}

	[Fact]
	public async Task GetProducers_WithEmptyIds_DoesNotFilterById()
	{
		var result = await Mediator.Send(CreateQuery([]), CancellationToken);

		result.Producers.Should().Contain(x => x.Id == Scenario.AlphaRequested.Id);
		result.Producers.Should().Contain(x => x.Id == Scenario.Gamma.Id);
	}

	[Fact]
	public async Task GetProducers_WithDuplicateAndMissingIds_ReturnsExistingProducerOnce()
	{
		var producer = Scenario.AlphaRequested;

		var result = await Mediator.Send(
			CreateQuery([producer.Id, producer.Id, int.MaxValue]),
			CancellationToken);

		result.Producers.Should().ContainSingle();
		result.Producers.Single().Id.Should().Be(producer.Id);
	}

	[Fact]
	public async Task GetProducers_WithIdsAndSearchTerm_AppliesBothFilters()
	{
		var result = await Mediator.Send(
			CreateQuery([Scenario.AlphaRequested.Id, Scenario.BetaRequested.Id], "Alpha"),
			CancellationToken);

		result.Producers.Should().ContainSingle();
		result.Producers.Single().Id.Should().Be(Scenario.AlphaRequested.Id);
		result.Producers.Should().NotContain(x => x.Id == Scenario.AlphaSkipped.Id);
	}

	[Fact]
	public async Task GetProducers_WithIds_AppliesPaginationAfterFiltering()
	{
		var result = await Mediator.Send(
			CreateQuery(
				[Scenario.AlphaRequested.Id, Scenario.BetaRequested.Id, Scenario.Gamma.Id],
				page: 1,
				size: 1),
			CancellationToken);

		result.Producers.Should().ContainSingle();
		result.Producers.Single().Id.Should().Be(Scenario.BetaRequested.Id);
	}

	private static GetProducersQuery CreateQuery(
		IReadOnlyCollection<int> ids,
		string? searchTerm = null,
		int page = 0,
		int size = 100)
		=> new(
			searchTerm,
			ids,
			new Pagination(page, size));

	public sealed class ProducersScenario : ReadScenario
	{
		public Producer AlphaRequested { get; private set; } = null!;

		public Producer BetaRequested { get; private set; } = null!;

		public Producer AlphaSkipped { get; private set; } = null!;

		public Producer Gamma { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			AlphaRequested = new ProducerBuilder(Faker).WithName("Alpha requested producer").Build();
			BetaRequested = new ProducerBuilder(Faker).WithName("Beta requested producer").Build();
			AlphaSkipped = new ProducerBuilder(Faker).WithName("Alpha skipped producer").Build();
			Gamma = new ProducerBuilder(Faker).WithName("Gamma producer").Build();
			var aardvark = new ProducerBuilder(Faker).WithName("Aardvark skipped producer").Build();
			return [AlphaRequested, BetaRequested, AlphaSkipped, Gamma, aardvark];
		}
	}
}
