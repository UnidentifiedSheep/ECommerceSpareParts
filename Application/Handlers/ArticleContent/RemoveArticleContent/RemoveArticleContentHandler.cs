using Application.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using MediatR;

namespace Application.Handlers.ArticleContent.RemoveArticleContent;

public record RemoveArticleContentCommand(int ArticleId, int InsideArticleId) : ICommand;

public class RemoveArticleContentHandler(IArticleContentRepository contentRepository, 
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveArticleContentCommand>
{
    public async Task<Unit> Handle(RemoveArticleContentCommand request, CancellationToken cancellationToken)
    {
        var content = await contentRepository.GetArticleContentAsync(request.ArticleId, request.InsideArticleId, true, cancellationToken)
            ?? throw new ArticleContentNotFoundException(request.ArticleId, request.InsideArticleId);
        unitOfWork.Remove(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}