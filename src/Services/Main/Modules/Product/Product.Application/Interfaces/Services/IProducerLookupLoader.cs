namespace Main.Application.Interfaces.Services;

public interface IProducerLookupLoader
{
	Task<IProducerLookup> Load(CancellationToken cancellationToken = default);
}
