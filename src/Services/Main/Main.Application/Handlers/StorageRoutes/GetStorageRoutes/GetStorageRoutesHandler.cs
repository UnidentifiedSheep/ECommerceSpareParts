using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.StorageRoutes.GetStorageRoutes;

public record GetStorageRoutesQuery(string? StorageFrom, string? StorageTo, bool? IsActive, 
    PaginationModel PaginationModel) : IQuery<GetStorageRoutesResult>;
public record GetStorageRoutesResult(List<StorageRouteDto> StorageRoutes);

public class GetStorageRoutesHandler(IStorageRoutesRepository storageRoutesRepository) 
    : IQueryHandler<GetStorageRoutesQuery, GetStorageRoutesResult>
{
    public async Task<GetStorageRoutesResult> Handle(GetStorageRoutesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PaginationModel.Page;
        var limit = request.PaginationModel.Size;
        var routes = await storageRoutesRepository
            .GetStorageRoutesAsync(request.StorageFrom, request.StorageTo, request.IsActive, 
                page, limit, false, cancellationToken, x => x.Currency);

        return new GetStorageRoutesResult(routes.Adapt<List<StorageRouteDto>>());
    }
}