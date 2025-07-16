using Core.Interface;
using FluentValidation;
using Mapster;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.Services.Catalogue;
using MonoliteUnicorn.Services.SearchLog;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleByNumberOrName;

public record GetArticleByNumberOrNameAnonymousQuery(string SearchTerm, int Page, int ViewCount, string? SortBy, IEnumerable<int> ProducerIds, string? UserId) : IQuery<GetArticleByNumberOrNameAnonymousResult>;
public record GetArticleByNumberOrNameAnonymousResult(IEnumerable<ArticleDto> Articles);

public class GetArticleByNumberOrNameAnonymousValidation : AbstractValidator<GetArticleByNumberOrNameAnonymousQuery>
{
    public GetArticleByNumberOrNameAnonymousValidation()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("Минимальная длинна строки поиска 3");
        
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}
public class GetArticleByNumberOrNameAnonymousHandler(ICatalogue catalogue, ISearchLogger searchLogger) : IQueryHandler<GetArticleByNumberOrNameAnonymousQuery, GetArticleByNumberOrNameAnonymousResult>
{
    public async Task<GetArticleByNumberOrNameAnonymousResult> Handle(GetArticleByNumberOrNameAnonymousQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var searchModel = new SearchLogModel(request.UserId, "Articles_FullSearch", request);
            searchLogger.Enqueue(searchModel);
        }

        var articles = await catalogue.GetArticlesByNameOrNumber(request.SearchTerm, request.Page, 
            request.ViewCount, request.SortBy, request.ProducerIds, cancellationToken);
        
        return new GetArticleByNumberOrNameAnonymousResult(articles.Adapt<List<ArticleDto>>());
    }
}