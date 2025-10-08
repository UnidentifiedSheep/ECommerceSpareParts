using Core.Attributes;
using Core.Entities;
using Core.Interfaces.Services;
using Main.Application.Interfaces;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticlePairs.CreatePair;

[Transactional]
public record CreatePairCommand(int LeftArticleId, int RightArticleId) : ICommand;

public class CreatePairHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePairCommand>
{
    public async Task<Unit> Handle(CreatePairCommand request, CancellationToken cancellationToken)
    {
        var leftPair = request.Adapt<ArticlesPair>();
        var rightPair = request.Adapt<ArticlesPair>();

        // переопределяем только для обратной пары
        rightPair.ArticleLeft = request.RightArticleId;
        rightPair.ArticleRight = request.LeftArticleId;

        await unitOfWork.AddRangeAsync([leftPair, rightPair], cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}