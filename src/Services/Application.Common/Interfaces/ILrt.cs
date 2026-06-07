namespace Application.Common.Interfaces;

public interface ILrt
{
    Task ExecuteAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
}