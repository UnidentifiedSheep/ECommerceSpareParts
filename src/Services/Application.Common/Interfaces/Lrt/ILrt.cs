namespace Application.Common.Interfaces.Lrt;

public interface ILrt
{
    Task ExecuteAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
}