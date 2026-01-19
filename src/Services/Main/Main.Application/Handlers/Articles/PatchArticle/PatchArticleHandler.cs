using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Notifications;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Articles.PatchArticle;

[Transactional]
public record PatchArticleCommand(int ArticleId, PatchArticleDto PatchArticle) : ICommand;

public class PatchArticleHandler(IMediator mediator, IArticlesRepository articlesRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<PatchArticleCommand>
{
    public async Task<Unit> Handle(PatchArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await articlesRepository.GetArticleById(request.ArticleId, true, cancellationToken)
                      ?? throw new ArticleNotFoundException(request.ArticleId);

        request.PatchArticle.Adapt(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new ArticleUpdatedNotification(request.ArticleId), cancellationToken);

        return Unit.Value;
    }
}