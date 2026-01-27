using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleWeight;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.Handlers.ArticleWeight.DeleteArticleWeight;

[Transactional]
public record DeleteArticleWeightCommand(int ArticleId) : ICommand;

public class DeleteArticleWeightHandler(IArticleWeightRepository weightRepository, IUnitOfWork unitOfWork, IMediator mediator) 
    : ICommandHandler<DeleteArticleWeightCommand>
{
    public async Task<Unit> Handle(DeleteArticleWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await weightRepository.GetArticleWeight(request.ArticleId, true, cancellationToken)
                     ?? throw new ArticleWeightNotFound(request.ArticleId);
        unitOfWork.Remove(weight);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        //publish notification to invalidate cache
        await mediator.Publish(new ArticleWeightUpdatedNotification(request.ArticleId), cancellationToken);
        return Unit.Value;
    }
}