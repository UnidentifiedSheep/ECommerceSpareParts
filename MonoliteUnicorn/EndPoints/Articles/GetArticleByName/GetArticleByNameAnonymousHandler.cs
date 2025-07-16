using Core.Interface;
using FluentValidation;
using Mapster;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.Services.Catalogue;
using MonoliteUnicorn.Services.SearchLog;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleByName;

public record GetArticleByNameAnonymousQuery(string SearchTerm, int Page, int ViewCount, string? SortBy, IEnumerable<int> ProducerIds, string? UserId) : IQuery<GetArticleByNameAnonymousResult>;
public record GetArticleByNameAnonymousResult(IEnumerable<ArticleDto> Articles);

public class GetArticleByNameAnonymousValidation : AbstractValidator<GetArticleByNameAnonymousQuery>
{
    public GetArticleByNameAnonymousValidation()
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

public class GetArticleByNameAnonymousHandler(ICatalogue catalogue, ISearchLogger searchLogger) : IQueryHandler<GetArticleByNameAnonymousQuery, GetArticleByNameAnonymousResult>
{
    public async Task<GetArticleByNameAnonymousResult> Handle(GetArticleByNameAnonymousQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var searchModel = new SearchLogModel(request.UserId, "Articles_ByName", request);
            searchLogger.Enqueue(searchModel);
        }
        
        var articles = await catalogue
            .GetArticlesByName(request.SearchTerm, request.Page, request.ViewCount, 
                request.SortBy, request.ProducerIds, cancellationToken);
        
        return new GetArticleByNameAnonymousResult(articles.Adapt<List<ArticleDto>>());
    }
}