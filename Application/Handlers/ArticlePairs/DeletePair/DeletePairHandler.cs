using Application.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using MediatR;

namespace Application.Handlers.ArticlePairs.DeletePair;

[Transactional]
public record DeletePairCommand(int ArticleId) : ICommand;

public class DeletePairHandler(IArticlePairsRepository pairsRepository, IUnitOfWork unitOfWork) : ICommandHandler<DeletePairCommand>
{
    public async Task<Unit> Handle(DeletePairCommand request, CancellationToken cancellationToken)
    {
        var pairs = await pairsRepository.GetRelatedPairsAsync(request.ArticleId, true, cancellationToken);
        unitOfWork.RemoveRange(pairs);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}