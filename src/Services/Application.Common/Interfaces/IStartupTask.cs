namespace Application.Common.Interfaces;

public interface IStartupTask
{
    Task ExecuteAsync(CancellationToken ct);
}