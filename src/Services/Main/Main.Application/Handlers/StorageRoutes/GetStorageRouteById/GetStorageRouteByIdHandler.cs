using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Projections;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Storage;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRouteById;

public record GetStorageRouteByIdQuery(Guid Id) : IQuery<GetStorageRouteByIdResult>;

public record GetStorageRouteByIdResult(StorageRouteDto StorageRoute);

public class GetStorageRouteByIdHandler(
    IReadRepository<StorageRoute, Guid> repository)
    : IQueryHandler<GetStorageRouteByIdQuery, GetStorageRouteByIdResult>
{
    public async Task<GetStorageRouteByIdResult> Handle(
        GetStorageRouteByIdQuery request,
        CancellationToken cancellationToken)
    {
        var route = await repository.Query
            .AsExpandable()
            .Select(StorageProjections.StorageRouteProjection)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new StorageRouteNotFound(request.Id);
        return new GetStorageRouteByIdResult(route);
    }
}