namespace Main.Modules.Product.Application.Interfaces.Services;

public interface IProducerLookupLoader
{
	Task<IProducerLookup> Load(CancellationToken cancellationToken = default);
}
