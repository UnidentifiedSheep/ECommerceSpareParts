namespace Application.Common.Interfaces.Lrt;

public interface ILrt
{
    Task ExecuteAsync(
        Guid jobId,
        Guid leaseHolderId,
        CancellationToken cancellationToken = default);
}