using Main.Application.Models.Producer;

namespace Main.Application.Interfaces.Services;

public interface IProducerLookupService
{
    Task<ProducerLookup> Load(CancellationToken cancellationToken = default);
}
