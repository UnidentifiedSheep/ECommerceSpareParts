namespace Main.Core.Interfaces.Pricing;

public interface IPriceSetup
{
    Task SetupAsync(CancellationToken cancellationToken = default);
}