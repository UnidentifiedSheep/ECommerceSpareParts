using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleSizes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.Handlers.ArticleSizes.DeleteArticleSizes;

[Transactional]
public record DeleteArticleSizesCommand(int ArticleId) : ICommand;

public class DeleteArticleSizesHandler(IArticleSizesRepository sizesRepository, IUnitOfWork unitOfWork, IMediator mediator) 
    : ICommandHandler<DeleteArticleSizesCommand>
{
    public async Task<Unit> Handle(DeleteArticleSizesCommand request, CancellationToken cancellationToken)
    {
        var sizes = await sizesRepository.GetArticleSizes(request.ArticleId, true, cancellationToken)
                    ?? throw new ArticleSizesNotFoundException(request.ArticleId);
        unitOfWork.Remove(sizes);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        //publish notification to invalidate cache
        await mediator.Publish(new ArticleSizeUpdatedNotification(request.ArticleId), cancellationToken);
        return Unit.Value;
    }
}