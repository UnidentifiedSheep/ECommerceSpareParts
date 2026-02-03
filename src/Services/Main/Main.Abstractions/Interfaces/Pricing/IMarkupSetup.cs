namespace Main.Abstractions.Interfaces.Pricing;

public interface IMarkupSetup
{
    Task SetupAsync(CancellationToken cancellationToken = default);
}