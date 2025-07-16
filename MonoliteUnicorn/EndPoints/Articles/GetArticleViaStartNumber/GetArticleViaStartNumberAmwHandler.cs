using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Catalogue;
using MonoliteUnicorn.Services.SearchLog;
using ArticleDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleDto;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaStartNumber;

public partial record GetArticleViaStartNumberAmwQuery(string ArticleNumber, int ViewCount, int Page, string? SortBy, IEnumerable<int> ProducerIds, string? UserId) : IQuery<GetArticleViaStartNumberAmwResult>;
public record GetArticleViaStartNumberAmwResult(IEnumerable<ArticleDto> Articles);
public class GetArticleViaExecNumberAmwValidation : AbstractValidator<GetArticleViaStartNumberAmwQuery>
{
    public GetArticleViaExecNumberAmwValidation()
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

public class GetArticleViaStartNumberAmwHandler(ICatalogue catalogue, ISearchLogger searchLogger) : IQueryHandler<GetArticleViaStartNumberAmwQuery, GetArticleViaStartNumberAmwResult>
{
    public async Task<GetArticleViaStartNumberAmwResult> Handle(GetArticleViaStartNumberAmwQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var searchModel = new SearchLogModel(request.UserId, "Articles_ExecNumber", request);
            searchLogger.Enqueue(searchModel);
        }

        var articles = await catalogue.GetArticlesByStartNumber(request.ArticleNumber, request.Page, request.ViewCount,
            request.SortBy, request.ProducerIds, cancellationToken);
        return new GetArticleViaStartNumberAmwResult(articles.Adapt<List<ArticleDto>>());
    }
}