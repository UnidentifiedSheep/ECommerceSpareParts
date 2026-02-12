using Contracts.Models.ArticleCoefficients;

namespace Contracts.ArticleCoefficients.GetArticleCoefficients;

public record GetArticleCoefficientsResponse
{
    public Dictionary<int, ArticleCoefficient> Coefficients { get; init; } = [];

}