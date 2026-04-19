using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Application.Handlers.Currencies.Projections;
using Main.Entities.Storage;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRoutes;

public record GetStorageRoutesQuery(
    string? StorageFrom,
    string? StorageTo,
    bool? IsActive,
    PaginationModel PaginationModel) : IQuery<GetStorageRoutesResult>;

public record GetStorageRoutesResult(List<StorageRouteDto> StorageRoutes);

public class GetStorageRoutesHandler(
    IReadRepository<StorageRoute, Guid> repository)
    : IQueryHandler<GetStorageRoutesQuery, GetStorageRoutesResult>
{
    public async Task<GetStorageRoutesResult> Handle(GetStorageRoutesQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query;
        
        if (!string.IsNullOrWhiteSpace(request.StorageFrom))
            query = query.Where(x => x.FromStorageName == request.StorageFrom);
        if (!string.IsNullOrWhiteSpace(request.StorageTo))
            query = query.Where(x => x.ToStorageName == request.StorageTo);
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive);
        
        query = query.ApplyPagination(request.PaginationModel);

        var routes = await query
            .AsExpandable()
            .Select(StorageProjections.StorageRouteProjection)
            .ToListAsync(cancellationToken);

        return new GetStorageRoutesResult(routes);
    }
}