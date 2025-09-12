using Application.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using MediatR;

namespace Application.Handlers.ArticleContent.SetArticleContentCount;

public record SetArticlesContentCountCommand(int ArticleId, int InsideArticleId, int Count) : ICommand;

public class SetArticlesContentCountHandler(
    IArticleContentRepository contentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetArticlesContentCountCommand>
{
    public async Task<Unit> Handle(SetArticlesContentCountCommand request, CancellationToken cancellationToken)
    {
        var content =
            await contentRepository.GetArticleContentAsync(request.ArticleId, request.InsideArticleId, true,
                cancellationToken)
            ?? throw new ArticleContentNotFoundException(request.ArticleId, request.InsideArticleId);
        content.Quantity = request.Count;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}