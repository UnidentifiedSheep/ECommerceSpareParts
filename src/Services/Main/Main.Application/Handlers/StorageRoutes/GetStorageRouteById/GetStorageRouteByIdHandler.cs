using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRouteById;

public record GetStorageRouteByIdQuery(Guid Id) : IQuery<GetStorageRouteByIdResult>;

public record GetStorageRouteByIdResult(StorageRouteDto StorageRoute);

public class GetStorageRouteByIdHandler(
    IReadRepository<StorageRoute, Guid> repository,
    IProjectionProvider<StorageRoute, StorageRouteDto> projection
)
    : IQueryHandler<GetStorageRouteByIdQuery, GetStorageRouteByIdResult>
{
    public async Task<GetStorageRouteByIdResult> Handle(
        GetStorageRouteByIdQuery request,
        CancellationToken cancellationToken)
    {
        var route = await repository.Query
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new StorageRouteNotFound(request.Id);
        return new GetStorageRouteByIdResult(route);
    }
}
