using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Member.Articles;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.Price;
using MonoliteUnicorn.Services.SearchLog;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleCrosses;

public record GetArticleCrossesMemberQuery(int ArticleId, int ViewCount, int Page, string? SortBy, int? CurrencyId, string UserId) : IQuery<GetArticleCrossesMemberResult>;
public record GetArticleCrossesMemberResult(IEnumerable<ArticleFullDto> Crosses, ArticleFullDto RequestedArticle);

public class GetArticleCrossesMemberValidation : AbstractValidator<GetArticleCrossesMemberQuery>
{
    public GetArticleCrossesMemberValidation()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetArticleCrossesMemberHandler(DContext context, IPrice priceService, ISearchLogger searchLogger) : IQueryHandler<GetArticleCrossesMemberQuery, GetArticleCrossesMemberResult>
{
    public async Task<GetArticleCrossesMemberResult> Handle(GetArticleCrossesMemberQuery request, CancellationToken cancellationToken)
    {
        var searchModel = new SearchLogModel(request.UserId, "ArticleCrosses", request);
        searchLogger.Enqueue(searchModel);
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
            .Include(x => x.ArticleImages)
            .Skip(request.Page * request.ViewCount)
            .Take(request.ViewCount)
            .AsNoTracking()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
        var crossArticles = crosses.Adapt<List<ArticleFullDto>>();
        var requestedArt = requestedArticle.Adapt<ArticleFullDto>();
        
        if (request.CurrencyId == null) return new GetArticleCrossesMemberResult(crossArticles, requestedArt);
        
        var prices = await priceService
            .GetPrices(crossArticles.Select(x => x.Id), request.CurrencyId ?? 1, request.UserId, cancellationToken);
        foreach (var item in crossArticles)
        {
            if (!prices.TryGetValue(item.Id, out var price)) continue;
            item.Price = (decimal)price;
            item.CurrencyId = request.CurrencyId ?? 1;
            if (item.Id != requestedArt.Id) continue;
            requestedArt.Price = (decimal)price;
            item.CurrencyId = request.CurrencyId ?? 1;
        }
        return new GetArticleCrossesMemberResult(crossArticles, requestedArt);
    }
}