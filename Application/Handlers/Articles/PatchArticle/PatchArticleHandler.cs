using Application.Events;
using Application.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.Articles;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Mapster;
using MediatR;

namespace Application.Handlers.Articles.PatchArticle;

[Transactional]
public record PatchArticleCommand(int ArticleId, PatchArticleDto PatchArticle) : ICommand;

public class PatchArticleHandler(IMediator mediator, IArticlesRepository articlesRepository, IUnitOfWork unitOfWork) : ICommandHandler<PatchArticleCommand>
{
    public async Task<Unit> Handle(PatchArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await articlesRepository.GetArticleById(request.ArticleId, true, cancellationToken)
                      ?? throw new ArticleNotFoundException(request.ArticleId);
        
        request.PatchArticle.Adapt(article);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new ArticleUpdatedEvent(request.ArticleId), cancellationToken);
        
        return Unit.Value;
    }
}