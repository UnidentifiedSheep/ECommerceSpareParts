using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.Price;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleCrosses;


public record GetArticleCrossesAmwQuery(int ArticleId, int ViewCount, int Page, string? SortBy, int? CurrencyId) : IQuery<GetArticleCrossesAmwResult>;
public record GetArticleCrossesAmwResult(IEnumerable<ArticleFullDto> Crosses, ArticleFullDto RequestedArticle);

public class GetArticleCrossesAmwValidation : AbstractValidator<GetArticleCrossesAmwQuery>
{
    public GetArticleCrossesAmwValidation()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetArticleCrossesAmwHandler(DContext context, IPrice priceService) : IQueryHandler<GetArticleCrossesAmwQuery, GetArticleCrossesAmwResult>
{
    public async Task<GetArticleCrossesAmwResult> Handle(GetArticleCrossesAmwQuery request, CancellationToken cancellationToken)
    {
        var requestedArticle = await context.Articles.AsNoTracking()
            .Include(a => a.Producer)
            .Include(x => x.ArticleImages)
            .FirstOrDefaultAsync(x => x.Id == request.ArticleId, cancellationToken);
        if (requestedArticle == null) throw new ArticleNotFoundException(request.ArticleId);
        var crosses = await context.Articles
            .FromSql($"""
                      SELECT Distinct on (a.id) a.* 
                      FROM articles a 
                      JOIN article_crosses c ON a.id = c.article_id OR a.id = c.article_cross_id 
                                             WHERE c.article_id = {request.ArticleId} OR 
                                                 c.article_cross_id = {request.ArticleId} 
                      """)
            .SortBy(request.SortBy)
            .Skip(request.Page * request.ViewCount)
            .Include(x => x.ArticleImages)
            .Take(request.ViewCount)
            .AsNoTracking()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken: cancellationToken);
        var crossArticles = crosses.Adapt<List<ArticleFullDto>>();
        var requestedArt = requestedArticle.Adapt<ArticleFullDto>();
        if (request.CurrencyId == null) return new GetArticleCrossesAmwResult(crossArticles, requestedArt);
        
        var prices = await priceService.GetDetailedPrices(crossArticles.Select(x => x.Id), request.CurrencyId ?? 1, null, cancellationToken);
        foreach (var item in crossArticles)
        {
            if (!prices.TryGetValue(item.Id, out var info)) continue;
            item.DetailedPrice = info;
            if (item.Id == requestedArt.Id) requestedArt.DetailedPrice = info;
        }
        
        return new GetArticleCrossesAmwResult(crossArticles, requestedArt);
    }
}