namespace Main.Application.Interfaces.Services.Event;

public interface ISaleEventService
{
    Task NotifyUpdated(
        Guid id,
        CancellationToken cancellationToken);
}