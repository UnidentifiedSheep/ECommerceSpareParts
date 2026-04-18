using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Exceptions.Storages;
using Main.Application.Handlers.Currencies.Projections;
using Main.Application.Interfaces.Repositories;
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
            .Select(StorageProjections.StorageRouteProjection)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new StorageRouteNotFound(request.Id);
        return new GetStorageRouteByIdResult(route);
    }
}