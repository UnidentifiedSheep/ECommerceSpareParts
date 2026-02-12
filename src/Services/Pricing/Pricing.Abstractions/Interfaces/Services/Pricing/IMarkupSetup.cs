namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IMarkupSetup
{
    Task SetupAsync(CancellationToken cancellationToken = default);
}