using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.ArticleCoefficients;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.ArticleCoefficient.GetArticleCoefficients;

public record GetArticleCoefficientsQuery(IEnumerable<int> ArticleIds) : IQuery<GetArticleCoefficientsResult>;

public record GetArticleCoefficientsResult(Dictionary<int, List<ArticleCoefficientDto>> Coefficients);

public class GetArticleCoefficientsHandler(IArticleCoefficients articleCoefficientsRepository) 
    : IQueryHandler<GetArticleCoefficientsQuery, GetArticleCoefficientsResult>
{
    public async Task<GetArticleCoefficientsResult> Handle(GetArticleCoefficientsQuery request, CancellationToken cancellationToken)
    {
        var coefficients = await articleCoefficientsRepository
            .GetArticlesCoefficients(request.ArticleIds, false, cancellationToken, 
                x => x.CoefficientNameNavigation);

        var result = coefficients
            .ToDictionary(x => x.Key,
                x => x.Value.Adapt<List<ArticleCoefficientDto>>());
        return new GetArticleCoefficientsResult(result);
    }
}