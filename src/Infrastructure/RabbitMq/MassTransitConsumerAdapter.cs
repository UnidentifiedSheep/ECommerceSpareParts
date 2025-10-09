using Core.Interfaces.MessageBroker;
using MassTransit;

namespace RabbitMq;

public class MassTransitConsumerAdapter<TEvent> : IConsumer<TEvent> where TEvent : class
{
    private readonly IEventHandler<TEvent> _handler;

    public MassTransitConsumerAdapter(IEventHandler<TEvent> handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        // Если нужен IEventContext, можно обернуть context в адаптер
        await _handler.HandleAsync(new MassTransitEventContext<TEvent>(context));
    }
}