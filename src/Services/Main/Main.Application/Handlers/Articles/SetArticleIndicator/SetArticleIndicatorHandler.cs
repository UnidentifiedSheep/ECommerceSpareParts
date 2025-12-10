using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Application.Notifications;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Articles.SetArticleIndicator;

[Transactional]
[ExceptionType<ArticleNotFoundException>]
public record SetArticleIndicatorCommand(int ArticleId, string? Indicator) : ICommand;

public class SetArticleIndicatorHandler(
    IMediator mediator,
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetArticleIndicatorCommand>
{
    public async Task<Unit> Handle(SetArticleIndicatorCommand request, CancellationToken cancellationToken)
    {
        var article = await articlesRepository.GetArticleById(request.ArticleId, true, cancellationToken)
                      ?? throw new ArticleNotFoundException(request.ArticleId);
        article.Indicator = request.Indicator;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new ArticleUpdatedNotification(request.ArticleId), cancellationToken);

        return Unit.Value;
    }
}