using Core.Interface;
using FluentValidation;
using Mapster;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.Services.Catalogue;
using MonoliteUnicorn.Services.SearchLog;
using ArticleDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleDto;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleByName;

public record GetArticleByNameAmwQuery(string SearchTerm, int Page, int ViewCount, string? SortBy, IEnumerable<int> ProducerIds, string? UserId) : IQuery<GetArticleByNameAmwResult>;
public record GetArticleByNameAmwResult(IEnumerable<ArticleDto> Articles);

public class GetArticleByNameAmwValidation : AbstractValidator<GetArticleByNameAmwQuery>
{
    public GetArticleByNameAmwValidation()
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

public class GetArticleByNameAmwHandler(ICatalogue catalogue, ISearchLogger searchLogger) : IQueryHandler<GetArticleByNameAmwQuery, GetArticleByNameAmwResult>
{
    public async Task<GetArticleByNameAmwResult> Handle(GetArticleByNameAmwQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var searchModel = new SearchLogModel(request.UserId, "Articles_ByName", request);
            searchLogger.Enqueue(searchModel);
        }
        
        var articles = await catalogue
            .GetArticlesByName(request.SearchTerm, request.Page, request.ViewCount, 
                request.SortBy, request.ProducerIds, cancellationToken);
        
        return new GetArticleByNameAmwResult(articles.Adapt<List<ArticleDto>>());
    }
}