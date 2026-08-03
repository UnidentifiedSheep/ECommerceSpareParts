using Main.Application.Models.Producer;

namespace Main.Application.Interfaces.Services;

public interface ISupplierProducerLookupService
{
    Task<SupplierProducerLookup> Load(
        CancellationToken cancellationToken = default);
}
