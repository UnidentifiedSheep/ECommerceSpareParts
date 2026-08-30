using FluentAssertions;
using Main.Application.Handlers.Producers;
using Main.Entities.Exceptions;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Producers;

public class GetProducerByIdTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task GetProducerById_ExistingProducer_ReturnsProducer()
	{
		var producer = await new ProducerBuilder(Faker)
			.WithName("Requested producer")
			.WithDescription("Requested description")
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(new GetProducersByIdsQuery(producer.Id));

		result.Producer.Id.Should().Be(producer.Id);
		result.Producer.Name.Should().Be(producer.Name);
		result.Producer.Description.Should().Be(producer.Description);
	}

	[Fact]
	public async Task GetProducerById_MissingProducer_ThrowsProducerNotFoundException()
	{
		var act = () => Mediator.Send(new GetProducersByIdsQuery(int.MaxValue));

		await act.Should().ThrowAsync<ProducerNotFoundException>();
	}
}
