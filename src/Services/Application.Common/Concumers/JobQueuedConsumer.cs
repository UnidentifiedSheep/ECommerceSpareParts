using Application.Common.Interfaces.Lrt;
using Contracts.Lrt;
using MassTransit;

namespace Application.Common.Concumers;

public class JobQueuedConsumer(ILrtService lrtService) : IConsumer<JobQueuedEvent>
{
    public Task Consume(ConsumeContext<JobQueuedEvent> context)
    {
        return lrtService.RunLrtAsync(context.Message.JobId);
    }
}