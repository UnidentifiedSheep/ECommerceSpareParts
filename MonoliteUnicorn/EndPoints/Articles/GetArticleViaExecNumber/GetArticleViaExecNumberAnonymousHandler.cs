using Core.Interface;
using FluentValidation;
using Mapster;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.Services.Catalogue;
using MonoliteUnicorn.Services.SearchLog;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaExecNumber;
public record GetArticleViaExecNumberAnonymousQuery(string ArticleNumber, int ViewCount, int Page, string? SortBy, IEnumerable<int> ProducerIds, string? UserId) : IQuery<GetArticleViaExecNumberAnonymousResult>;
public record GetArticleViaExecNumberAnonymousResult(IEnumerable<ArticleDto> Articles);

public class GetArticleViaExecNumberAnonymousValidation : AbstractValidator<GetArticleViaExecNumberAnonymousQuery>
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
public class GetArticleViaExecNumberAnonymousHandler(ICatalogue catalogue, ISearchLogger searchLogger) : IQueryHandler<GetArticleViaExecNumberAnonymousQuery, GetArticleViaExecNumberAnonymousResult>    
{
    public async Task<GetArticleViaExecNumberAnonymousResult> Handle(GetArticleViaExecNumberAnonymousQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var searchModel = new SearchLogModel(request.UserId, "Articles_ExecNumber", request);
            searchLogger.Enqueue(searchModel);
        }
        
        var articles = await catalogue.GetArticlesByExecNumber(request.ArticleNumber, request.Page, request.ViewCount, 
            request.SortBy, request.ProducerIds, cancellationToken);
        return new GetArticleViaExecNumberAnonymousResult(articles.Adapt<List<ArticleDto>>());
    }
}