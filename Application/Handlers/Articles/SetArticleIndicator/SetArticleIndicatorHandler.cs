using Application.Events;
using Application.Interfaces;
using Core.Attributes;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using MediatR;

namespace Application.Handlers.Articles.SetArticleIndicator;

[Transactional]
public record SetArticleIndicatorCommand(int ArticleId, string? Indicator) : ICommand;

public class SetArticleIndicatorHandler(IMediator mediator, IArticlesRepository articlesRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<SetArticleIndicatorCommand>
{
    public async Task<Unit> Handle(SetArticleIndicatorCommand request, CancellationToken cancellationToken)
    {
        var article = await articlesRepository.GetArticleById(request.ArticleId, true, cancellationToken)
                      ?? throw new ArticleNotFoundException(request.ArticleId);
        article.Indicator = request.Indicator;
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new ArticleUpdatedEvent(request.ArticleId), cancellationToken);
        
        return Unit.Value;
    }
}