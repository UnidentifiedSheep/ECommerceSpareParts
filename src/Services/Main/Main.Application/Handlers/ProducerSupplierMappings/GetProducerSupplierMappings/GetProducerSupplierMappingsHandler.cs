using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Enums;
using LinqKit;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Application.Projections;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProducerSupplierMappings.GetProducerSupplierMappings;

public record GetProducerSupplierMappingsQuery(
    int ProducerId,
    IEnumerable<Supplier> Suppliers,
    Pagination Pagination) : IQuery<GetProducerSupplierMappingsResult>;

public record GetProducerSupplierMappingsResult(
    IReadOnlyList<ProducerSupplierMappingDto> Mappings);

public class GetProducerSupplierMappingsHandler(
    IReadRepository<ProducerSupplierMapping, int> repository
    ) : IQueryHandler<GetProducerSupplierMappingsQuery, GetProducerSupplierMappingsResult>
{
    public async Task<GetProducerSupplierMappingsResult> Handle(
        GetProducerSupplierMappingsQuery request, 
        CancellationToken cancellationToken)
    {
        var query = repository.Query
            .Where(x => x.ProducerId == request.ProducerId);

        if (request.Suppliers.Any())
            query = query.Where(x => request.Suppliers.Contains(x.Supplier));

        var result = await query
            .AsExpandable()
            .Select(ProducerProjections.ToSupplierMappingDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetProducerSupplierMappingsResult(result);
    }
}