namespace Main.Abstractions.Interfaces.Pricing;

public interface IPriceSetup
{
    Task SetupAsync(CancellationToken cancellationToken = default);
}