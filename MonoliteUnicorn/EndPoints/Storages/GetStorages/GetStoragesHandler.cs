using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Storages.GetStorages;

public record GetStoragesQuery(int Page, int ViewCount, string? SearchTerm) : IQuery<GetStoragesResult>;
public record GetStoragesResult(IEnumerable<StorageDto> Storages);

public class GetStoragesValidation : AbstractValidator<GetStoragesQuery>
{
    public GetStoragesValidation()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetStoragesHandler(DContext context) : IQueryHandler<GetStoragesQuery, GetStoragesResult>
{
    public async Task<GetStoragesResult> Handle(GetStoragesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Storages.AsNoTracking();
        
        if(!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Select(x => new
                {
                    Entity = x,
                    Rank =
                        (EF.Functions.ILike(x.Name, $"%{request.SearchTerm}%") ? 3 : 0) +
                        (x.Description != null && EF.Functions.ILike(x.Description, $"%{request.SearchTerm}%") ? 2 : 0) +
                        (x.Location != null && EF.Functions.ILike(x.Location, $"%{request.SearchTerm}%") ? 1 : 0)
                })
                .Where(x => x.Rank > 0)
                .OrderByDescending(x => x.Rank)
                .Select(x => x.Entity);
        var result = await query
            .Skip(request.Page * request.ViewCount)
            .Take(request.ViewCount)
            .ToListAsync(cancellationToken: cancellationToken);
        return new GetStoragesResult(result.Adapt<List<StorageDto>>());
    }
}