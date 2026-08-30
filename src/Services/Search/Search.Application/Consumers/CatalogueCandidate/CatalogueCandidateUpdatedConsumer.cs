using Contracts.ProductEnrichment;
using MassTransit;
using MediatR;
using Search.Application.Handlers.CatalogueCandidates.UpsertCatalogueCandidates;

namespace Search.Application.Consumers.CatalogueCandidate;

public sealed class CatalogueCandidateUpdatedConsumer(ISender sender)
	: IConsumer<Batch<CatalogueCandidateUpdatedEvent>>
{
	public Task Consume(ConsumeContext<Batch<CatalogueCandidateUpdatedEvent>> context)
	{
		return sender.Send(
			new UpsertCatalogueCandidatesCommand(context.Message.Select(x => x.Message).ToList()),
			context.CancellationToken);
	}
}

public sealed class
	CatalogueCandidateUpdatedConsumerDefinition : ConsumerDefinition<CatalogueCandidateUpdatedConsumer>
{
	protected override void ConfigureConsumer(
		IReceiveEndpointConfigurator endpointConfigurator,
		IConsumerConfigurator<CatalogueCandidateUpdatedConsumer> consumerConfigurator,
		IRegistrationContext context)
	{
		consumerConfigurator.Options<BatchOptions>(options => options
			.SetMessageLimit(100)
			.SetTimeLimit(TimeSpan.FromSeconds(1))
			.SetConcurrencyLimit(1));
	}
}
