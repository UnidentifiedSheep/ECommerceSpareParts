using Application.Common.Interfaces;
using Core.Attributes;
using Exceptions.Exceptions.ArticlePair;
using Main.Core.Dtos.Anonymous.Articles;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.ArticlePairs.GetArticlePair;

public record GetArticlePairsQuery(int ArticleId) : IQuery<GetArticlePairsResult>;

public record GetArticlePairsResult(ArticleDto Pair);

public class GetArticlePairsHandler(IArticlePairsRepository pairsRepository)
    : IQueryHandler<GetArticlePairsQuery, GetArticlePairsResult>
{
    public async Task<GetArticlePairsResult> Handle(GetArticlePairsQuery request, CancellationToken cancellationToken)
    {
        var pair = await pairsRepository.GetArticlePairAsync(request.ArticleId, false, cancellationToken)
                   ?? throw new ArticlePairNotFoundException(request.ArticleId);
        var adapted = pair.Adapt<ArticleDto>();
        return new GetArticlePairsResult(adapted);
    }
}