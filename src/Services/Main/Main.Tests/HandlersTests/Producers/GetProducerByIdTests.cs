using FluentAssertions;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Queries.Producers;
using Tests.DataBuilders;

namespace Tests.HandlersTests.Producers;

public class GetProducerByIdTests(ReadScenarioFixture<GetProducerByIdTests.ProducerByIdScenario> fixture)
	: ReadScenarioTest<GetProducerByIdTests.ProducerByIdScenario>(fixture)
{
	[Fact]
	public async Task GetProducerById_ExistingProducer_ReturnsProducer()
	{
		var producer = Scenario.Producer;

		var result = await Mediator.Send(new GetProducersByIdsQuery(producer.Id), CancellationToken);

		result.Producer.Id.Should().Be(producer.Id);
		result.Producer.Name.Should().Be(producer.Name);
		result.Producer.Description.Should().Be(producer.Description);
	}

	[Fact]
	public async Task GetProducerById_MissingProducer_ThrowsProducerNotFoundException()
	{
		var act = () => Mediator.Send(new GetProducersByIdsQuery(int.MaxValue), CancellationToken);

		await act.Should().ThrowAsync<ProducerNotFoundException>();
	}

	public sealed class ProducerByIdScenario : ReadScenario
	{
		public Producer Producer { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			Producer = new ProducerBuilder(Faker)
				.WithName("Requested producer")
				.WithDescription("Requested description")
				.Build();
			return [Producer];
		}
	}
}
