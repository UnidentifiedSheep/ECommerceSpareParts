using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Producer;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services;

public sealed class SupplierProducerLookupService(
    IProducerLookupService producerLookupService,
    IReadRepository<ProducerSupplierMapping, int> mappingRepository)
    : ISupplierProducerLookupService
{
    public async Task<SupplierProducerLookup> Load(
        CancellationToken cancellationToken = default)
    {
        var producerLookup = await producerLookupService.Load(cancellationToken);
        var supplierMappingItems = await mappingRepository.Query
            .Select(x => new
            {
                x.Supplier,
                x.SupplierProducerName,
                x.ProducerId
            })
            .ToListAsync(cancellationToken);

        var supplierMappings = supplierMappingItems
            .ToDictionary(
                x => new SupplierProducerLookupKey(
                    x.Supplier,
                    x.SupplierProducerName),
                x => x.ProducerId);

        return new SupplierProducerLookup(
            producerLookup,
            supplierMappings);
    }
}
