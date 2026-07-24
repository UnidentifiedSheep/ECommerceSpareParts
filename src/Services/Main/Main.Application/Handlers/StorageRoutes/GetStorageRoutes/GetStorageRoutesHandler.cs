using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRoutes;

public record GetStorageRoutesQuery(
    string? StorageFrom,
    string? StorageTo,
    bool? IsActive,
    Pagination Pagination
) : IQuery<GetStorageRoutesResult>;

public record GetStorageRoutesResult(List<StorageRouteDto> StorageRoutes);

public class GetStorageRoutesHandler(
    IReadRepository<StorageRoute, Guid> repository,
    IProjectionProvider<StorageRoute, StorageRouteDto> projection
)
    : IQueryHandler<GetStorageRoutesQuery, GetStorageRoutesResult>
{
    public async Task<GetStorageRoutesResult> Handle(
        GetStorageRoutesQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (!string.IsNullOrWhiteSpace(request.StorageFrom))
            query = query.Where(x => x.FromStorageName == request.StorageFrom);
        if (!string.IsNullOrWhiteSpace(request.StorageTo))
            query = query.Where(x => x.ToStorageName == request.StorageTo);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);

        query = query.ApplyPagination(request.Pagination);

        var routes = await query
            .Project(projection)
            .ToListAsync(cancellationToken);

        return new GetStorageRoutesResult(routes);
    }
}
