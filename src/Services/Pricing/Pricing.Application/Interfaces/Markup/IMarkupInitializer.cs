namespace Pricing.Application.Interfaces.Markup;

public interface IMarkupInitializer
{
    Task Initialize(CancellationToken cancellationToken = default);
}