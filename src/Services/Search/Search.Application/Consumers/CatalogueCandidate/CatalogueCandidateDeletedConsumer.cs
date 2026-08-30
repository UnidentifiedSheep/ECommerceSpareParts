using Contracts.ProductEnrichment;
using MassTransit;
using MediatR;
using Search.Application.Handlers.CatalogueCandidates.DeleteCatalogueCandidates;

namespace Search.Application.Consumers.CatalogueCandidate;

public sealed class CatalogueCandidateDeletedConsumer(ISender sender)
	: IConsumer<Batch<CatalogueCandidateDeletedEvent>>
{
	public Task Consume(ConsumeContext<Batch<CatalogueCandidateDeletedEvent>> context)
	{
		return sender.Send(
			new DeleteCatalogueCandidatesCommand(context.Message.Select(x => x.Message.Id).ToList()),
			context.CancellationToken);
	}
}

public sealed class
	CatalogueCandidateDeletedConsumerDefinition : ConsumerDefinition<CatalogueCandidateDeletedConsumer>
{
	protected override void ConfigureConsumer(
		IReceiveEndpointConfigurator endpointConfigurator,
		IConsumerConfigurator<CatalogueCandidateDeletedConsumer> consumerConfigurator,
		IRegistrationContext context)
	{
		consumerConfigurator.Options<BatchOptions>(options => options
			.SetMessageLimit(100)
			.SetTimeLimit(TimeSpan.FromSeconds(1))
			.SetConcurrencyLimit(1));
	}
}
