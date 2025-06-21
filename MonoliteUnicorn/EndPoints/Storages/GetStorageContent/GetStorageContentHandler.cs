using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Storages.GetStorageContent;

public record GetStorageContentQuery(string? StorageName, int? ArticleId, int Page, int ViewCount, bool ShowZeroCount) : IQuery<GetStorageContentResult>;
public record GetStorageContentResult(IEnumerable<StorageContentDto> Content);

public class GetStorageContentValidation : AbstractValidator<GetStorageContentQuery>
{
    public GetStorageContentValidation()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetStorageContentHandler(DContext context) : IQueryHandler<GetStorageContentQuery, GetStorageContentResult>
{
    public async Task<GetStorageContentResult> Handle(GetStorageContentQuery request, CancellationToken cancellationToken)
    {
        var query = context.StorageContents.AsNoTracking()
            .Where(c => (string.IsNullOrWhiteSpace(request.StorageName) || c.StorageName == request.StorageName))
            .Where(x => (request.ArticleId == null || x.ArticleId == request.ArticleId));
        if (!request.ShowZeroCount) query = query.Where(x => x.Count > 0);
        var result = await query.Take(request.ViewCount)
            .Skip(request.Page * request.ViewCount)
            .ToListAsync(cancellationToken);
        return new GetStorageContentResult(result.Adapt<IEnumerable<StorageContentDto>>());
    }
}