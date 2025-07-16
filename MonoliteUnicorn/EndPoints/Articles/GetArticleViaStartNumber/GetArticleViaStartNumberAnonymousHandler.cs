using Core.Interface;
using FluentValidation;
using Mapster;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.Services.Catalogue;
using MonoliteUnicorn.Services.SearchLog;
using ArticleDto = MonoliteUnicorn.Dtos.Anonymous.Articles.ArticleDto;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaStartNumber;

public partial record GetArticleViaStartNumberAnonymousQuery(string ArticleNumber, int ViewCount, int Page, string? SortBy, IEnumerable<int> ProducerIds, string? UserId) : IQuery<GetArticleViaStartNumberAnonymousResult>;
public record GetArticleViaStartNumberAnonymousResult(IEnumerable<ArticleDto> Articles);
public class GetArticleViaExecNumberAnonymousValidation : AbstractValidator<GetArticleViaStartNumberAnonymousQuery>
{
    public GetArticleViaExecNumberAnonymousValidation()
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

public class GetArticleViaStartNumberAnonymousHandler(ICatalogue catalogue, ISearchLogger searchLogger) : IQueryHandler<GetArticleViaStartNumberAnonymousQuery, GetArticleViaStartNumberAnonymousResult>
{
    public async Task<GetArticleViaStartNumberAnonymousResult> Handle(GetArticleViaStartNumberAnonymousQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var searchModel = new SearchLogModel(request.UserId, "Articles_ExecNumber", request);
            searchLogger.Enqueue(searchModel);
        }

        var articles = await catalogue.GetArticlesByStartNumber(request.ArticleNumber, request.Page, request.ViewCount,
            request.SortBy, request.ProducerIds, cancellationToken);
        return new GetArticleViaStartNumberAnonymousResult(articles.Adapt<List<ArticleDto>>());
    }
}