using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProducerSupplierMappings;

public record GetProducersSupplierMappingsQuery(
    IEnumerable<int> ProducerIds) : IQuery<GetProducersSupplierMappingsResult>;

public record GetProducersSupplierMappingsResult(
    Dictionary<int, List<ProducerSupplierMappingDto>> Mappings);

public class GetProducersSupplierMappingsHandler(
    IReadRepository<ProducerSupplierMapping, int> repository,
    IProjectionProvider<ProducerSupplierMapping, ProducerSupplierMappingDto> projectionProvider
    ) : IQueryHandler<GetProducersSupplierMappingsQuery, GetProducersSupplierMappingsResult>
{
    public async Task<GetProducersSupplierMappingsResult> Handle(
        GetProducersSupplierMappingsQuery request, 
        CancellationToken cancellationToken)
    {
        var mappings = (await repository.Query
            .Where(x => request.ProducerIds.Contains(x.ProducerId))
            .Project(projectionProvider)
            .ToListAsync(cancellationToken))
            .GroupBy(x => x.ProducerId)
            .ToDictionary(
                x => x.Key, 
                x=> x.ToList());
        
        return new GetProducersSupplierMappingsResult(mappings);
    }
}