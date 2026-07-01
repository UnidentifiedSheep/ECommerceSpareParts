namespace Pricing.Application.Interfaces;

public interface IMarkupInitializer
{
    Task Initialize(CancellationToken cancellationToken = default);
}