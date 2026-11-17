namespace Pricing.Application.Interfaces.Markup;

public interface IMarkupRefresher
{
	Task Refresh(CancellationToken cancellationToken);
}
