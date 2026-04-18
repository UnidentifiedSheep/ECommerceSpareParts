using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Application.Handlers.Currencies.Projections;
using Main.Entities.Storage;
using Main.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Storages.GetStorage;

public record GetStoragesQuery(PaginationModel Pagination, string? SearchTerm, StorageType? Type)
    : IQuery<GetStoragesResult>;

public record GetStoragesResult(IReadOnlyList<StorageDto> Storages);

public class GetStoragesHandler(
    IReadRepository<Storage, string> repository
    ) : IQueryHandler<GetStoragesQuery, GetStoragesResult>
{
    public async Task<GetStoragesResult> Handle(GetStoragesQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query;
        var searchTerm = request.SearchTerm?.Trim();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Select(x => new
                {
                    Entity = x,
                    Rank =
                        (EF.Functions.ILike(x.Name, $"%{searchTerm}%") ? 3 : 0) +
                        (x.Description != null && EF.Functions.ILike(x.Description, $"%{searchTerm}%") ? 2 : 0) +
                        (x.Location != null && EF.Functions.ILike(x.Location, $"%{searchTerm}%") ? 1 : 0)
                })
                .Where(x => x.Rank > 0)
                .OrderByDescending(x => x.Rank)
                .Select(x => x.Entity);
        else
            query = query.OrderByDescending(x => x.Name);

        var result = await query
            .Where(x => request.Type == null || x.Type == request.Type)
            .Select(StorageProjections.StorageProjection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetStoragesResult(result);
    }
}