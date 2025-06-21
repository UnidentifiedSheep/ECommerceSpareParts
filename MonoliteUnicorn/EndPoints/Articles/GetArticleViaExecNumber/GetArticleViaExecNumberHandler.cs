using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaExecNumber;

public record GetArticleViaExecNumberQuery(string ArticleNumber, int ViewCount, int Page, string? SortBy,IEnumerable<int> ProducerIds) : IQuery<GetArticleViaExecNumberResult>;
public record GetArticleViaExecNumberResult(IEnumerable<ArticleDto> Articles);

public class GetArticleViaExecNumberValidation : AbstractValidator<GetArticleViaExecNumberQuery>
{
    public GetArticleViaExecNumberValidation()
    {
        RuleFor(x => x.ArticleNumber)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("Минимальная длинна артикула 3");
        
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}
public class GetArticleViaExecNumberHandler(DContext context) : IQueryHandler<GetArticleViaExecNumberQuery, GetArticleViaExecNumberResult>    
{
    public async Task<GetArticleViaExecNumberResult> Handle(GetArticleViaExecNumberQuery request, CancellationToken cancellationToken)
    {
        var normalizerArticle = request.ArticleNumber.ToNormalizedArticleNumber();
        
        var queryWithRank = context.Articles
            .AsNoTracking()
            .Where(a => EF.Functions.Like(a.NormalizedArticleNumber, $"{normalizerArticle}") &&
                        (!request.ProducerIds.Any() || request.ProducerIds.Contains(a.ProducerId)))
            .Select(x => new
            {
                Article = x,
                Rank = EF.Functions.TrigramsSimilarity(x.NormalizedArticleNumber, normalizerArticle)
            });
        
        if (string.IsNullOrWhiteSpace(request.SortBy) || request.SortBy.Contains("relevance"))
        {
            queryWithRank = request.SortBy?.Contains("asc") == true
                ? queryWithRank.OrderBy(x => x.Rank)
                : queryWithRank.OrderByDescending(x => x.Rank);
        }
        
        var query = queryWithRank
            .Select(x => x.Article);
        
        if (!string.IsNullOrWhiteSpace(request.SortBy) && !request.SortBy.Contains("relevance"))
            query = query.SortBy(request.SortBy);
        
        query = query
            .Skip(request.ViewCount * request.Page)
            .Take(request.ViewCount);

        var articles = await query
            .AsSplitQuery()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken);
        return new GetArticleViaExecNumberResult(articles.Adapt<List<ArticleDto>>());
    }
}