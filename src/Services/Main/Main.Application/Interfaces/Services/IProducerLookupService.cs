namespace Main.Application.Interfaces.Services;

public interface IProducerLookupService
{
    Task<IProducerLookup> Load(
        CancellationToken cancellationToken = default);
}
