using Application.Common.Interfaces;
using Main.Application.Dtos.Anonymous.Articles;
using Main.Entities.Product;
using Mapster;

namespace Main.Application.Handlers.ProductCharacteristics.GetCharacteristics;

public record GetArticleCharacteristicsQuery(int ArticleId, IEnumerable<int> CharacteristicsIds)
    : IQuery<GetArticleCharacteristicsResult>;

public record GetArticleCharacteristicsResult(IEnumerable<CharacteristicsDto> Characteristics);

public class GetCharacteristicsHandler(IArticleCharacteristicsRepository repository)
    : IQueryHandler<GetArticleCharacteristicsQuery, GetArticleCharacteristicsResult>
{
    public async Task<GetArticleCharacteristicsResult> Handle(
        GetArticleCharacteristicsQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<ProductCharacteristic> character;
        if (request.CharacteristicsIds.Any())
            character = await repository
                .GetArticleCharacteristicsByIds(request.ArticleId, request.CharacteristicsIds, false,
                    cancellationToken);
        else
            character = await repository.GetArticleCharacteristics(request.ArticleId, false, cancellationToken);
        return new GetArticleCharacteristicsResult(character.Adapt<List<CharacteristicsDto>>());
    }
}