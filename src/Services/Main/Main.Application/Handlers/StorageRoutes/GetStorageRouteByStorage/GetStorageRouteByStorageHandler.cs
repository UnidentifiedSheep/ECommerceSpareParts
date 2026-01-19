using Application.Common.Interfaces;
using Exceptions.Exceptions.StorageRoutes;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRouteByStorage;

public record GetStorageRouteByStorageQuery(string StorageFrom, string StorageTo) : IQuery<GetStorageRouteByStorageResult>;
public record GetStorageRouteByStorageResult(StorageRouteDto StorageRoute);

public class GetStorageRouteByStorageHandler(IStorageRoutesRepository storageRoutesRepository) 
    : IQueryHandler<GetStorageRouteByStorageQuery, GetStorageRouteByStorageResult>
{
    public async Task<GetStorageRouteByStorageResult> Handle(GetStorageRouteByStorageQuery request, CancellationToken cancellationToken)
    {
        var route = await storageRoutesRepository.GetStorageRouteAsync(request.StorageFrom, request.StorageTo, false,
            cancellationToken) ?? throw new StorageRouteNotFound(request.StorageFrom, request.StorageTo);

        return new GetStorageRouteByStorageResult(route.Adapt<StorageRouteDto>());
    }
}