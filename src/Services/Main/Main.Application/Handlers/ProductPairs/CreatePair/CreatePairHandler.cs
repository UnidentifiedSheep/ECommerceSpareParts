using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticlePairs.CreatePair;

[Transactional]
public record CreatePairCommand(int LeftArticleId, int RightArticleId) : ICommand;

public class CreatePairHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreatePairCommand>
{
    public async Task<Unit> Handle(CreatePairCommand request, CancellationToken cancellationToken)
    {
        var leftPair = request.Adapt<ProductPair>();
        var rightPair = request.Adapt<ProductPair>();

        // переопределяем только для обратной пары
        rightPair.Left = request.RightArticleId;
        rightPair.Right = request.LeftArticleId;

        await unitOfWork.AddRangeAsync([leftPair, rightPair], cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}