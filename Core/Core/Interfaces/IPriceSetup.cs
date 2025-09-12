namespace Core.Interfaces;

public interface IPriceSetup
{
    Task SetupAsync(CancellationToken cancellationToken = default);
}