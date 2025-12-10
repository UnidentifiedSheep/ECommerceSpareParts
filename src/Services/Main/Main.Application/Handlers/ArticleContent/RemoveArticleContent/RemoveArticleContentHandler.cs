using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.ArticleContent.RemoveArticleContent;

[Transactional]
[ExceptionType<ArticleContentNotFoundException>]
public record RemoveArticleContentCommand(int ArticleId, int InsideArticleId) : ICommand;

public class RemoveArticleContentHandler(
    IArticleContentRepository contentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveArticleContentCommand>
{
    public async Task<Unit> Handle(RemoveArticleContentCommand request, CancellationToken cancellationToken)
    {
        var content =
            await contentRepository.GetArticleContent(request.ArticleId, request.InsideArticleId, true,
                cancellationToken)
            ?? throw new ArticleContentNotFoundException(request.ArticleId, request.InsideArticleId);
        unitOfWork.Remove(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}