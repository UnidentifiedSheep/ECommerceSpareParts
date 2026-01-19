using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.ArticleContent.RemoveArticleContent;

[Transactional]
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