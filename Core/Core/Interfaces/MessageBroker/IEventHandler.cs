namespace Core.Interfaces.MessageBroker;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(IEventContext<TEvent> context);
}