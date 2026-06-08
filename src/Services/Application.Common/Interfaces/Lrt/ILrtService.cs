using Domain.CommonEntities;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtService
{
    Task RunLrtAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task QueueJob(
        Job job,
        CancellationToken cancellationToken = default);
}