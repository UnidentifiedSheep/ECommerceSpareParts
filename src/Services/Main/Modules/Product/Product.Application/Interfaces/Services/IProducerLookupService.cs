namespace Main.Modules.Product.Application.Interfaces.Services;

public interface IProducerLookupService
{
	Task<IProducerLookup> Load(CancellationToken cancellationToken = default);

	void Invalidate();
}
