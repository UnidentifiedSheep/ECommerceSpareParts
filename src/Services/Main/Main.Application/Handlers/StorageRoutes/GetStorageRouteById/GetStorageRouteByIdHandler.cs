using Application.Common.Interfaces;
using Exceptions.Exceptions.StorageRoutes;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRouteById;

public record GetStorageRouteByIdQuery(Guid Id) : IQuery<GetStorageRouteByIdResult>;
public record GetStorageRouteByIdResult(StorageRouteDto StorageRoute);

public class GetStorageRouteByIdHandler(IStorageRoutesRepository storageRoutesRepository) 
    : IQueryHandler<GetStorageRouteByIdQuery, GetStorageRouteByIdResult>
{
    public async Task<GetStorageRouteByIdResult> Handle(GetStorageRouteByIdQuery request, CancellationToken cancellationToken)
    {
        var route = await storageRoutesRepository.GetStorageRouteAsync(request.Id, false, cancellationToken)
                    ?? throw new StorageRouteNotFound(request.Id);
        return new GetStorageRouteByIdResult(route.Adapt<StorageRouteDto>());
    }
}