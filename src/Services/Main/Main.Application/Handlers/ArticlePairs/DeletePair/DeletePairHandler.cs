using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticlePair;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.ArticlePairs.DeletePair;

[Transactional]
[ExceptionType<ArticlePairNotFoundException>]
public record DeletePairCommand(int ArticleId) : ICommand;

public class DeletePairHandler(IArticlePairsRepository pairsRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePairCommand>
{
    public async Task<Unit> Handle(DeletePairCommand request, CancellationToken cancellationToken)
    {
        var pairs = (await pairsRepository
            .GetRelatedPairsAsync(request.ArticleId, true, cancellationToken))
            .ToList();

        if (pairs.Count == 0) throw new ArticlePairNotFoundException(request.ArticleId);
        
        unitOfWork.RemoveRange(pairs);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}