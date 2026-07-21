using Contracts.Storage;
using MassTransit;
using MediatR;
using Pricing.Application.Handlers.Pricing;

namespace Pricing.Application.Consumers;

public class StorageContentUpdatedConsumer(
    ISender sender
    ) : IConsumer<Batch<StorageContentUpdatedEvent>>
{
    public Task Consume(ConsumeContext<Batch<StorageContentUpdatedEvent>> context)
    {
        var items = context
            .Message
            .Select(x => x.Message)
            .ToList();

        return sender.Send(
            new ApplyOurPositionsCommand(items),
            context.CancellationToken);
    }
}

public class StorageContentUpdatedDefinition
    : ConsumerDefinition<StorageContentUpdatedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<StorageContentUpdatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(1))
            .SetConcurrencyLimit(1));
    }
}