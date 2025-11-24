using Application.Common.Interfaces;
using Main.Core.Dtos.Anonymous.Articles;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.ArticleCharacteristics.GetCharacteristics;

public record GetArticleCharacteristicsQuery(int ArticleId, IEnumerable<int> CharacteristicsIds) : IQuery<GetArticleCharacteristicsResult>;

public record GetArticleCharacteristicsResult(IEnumerable<CharacteristicsDto> Characteristics);

public class GetCharacteristicsHandler(IArticleCharacteristicsRepository repository)
    : IQueryHandler<GetArticleCharacteristicsQuery, GetArticleCharacteristicsResult>
{
    public async Task<GetArticleCharacteristicsResult> Handle(GetArticleCharacteristicsQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<ArticleCharacteristic> character;
        if (request.CharacteristicsIds.Any())
            character = await repository
                .GetArticleCharacteristicsByIds(request.ArticleId, request.CharacteristicsIds,false, cancellationToken);
        else
            character = await repository.GetArticleCharacteristics(request.ArticleId, false, cancellationToken);
        return new GetArticleCharacteristicsResult(character.Adapt<List<CharacteristicsDto>>());
    }
}