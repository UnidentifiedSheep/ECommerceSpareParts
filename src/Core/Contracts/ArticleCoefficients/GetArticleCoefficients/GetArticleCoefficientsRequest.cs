namespace Contracts.ArticleCoefficients.GetArticleCoefficients;

public record GetArticleCoefficientsRequest
{
    public IEnumerable<int> ArticleIds { get; init; } = null!;
}