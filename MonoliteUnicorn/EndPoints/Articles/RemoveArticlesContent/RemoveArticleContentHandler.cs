using Core.Interface;
using MediatR;
using MonoliteUnicorn.Services.Catalogue;

namespace MonoliteUnicorn.EndPoints.Articles.RemoveArticlesContent;

public record RemoveArticleContentCommand(int ArticleId, int InsideArticleId) : ICommand;

public class RemoveArticleContentHandler(ICatalogue catalogue) : ICommandHandler<RemoveArticleContentCommand>
{
    public async Task<Unit> Handle(RemoveArticleContentCommand request, CancellationToken cancellationToken)
    {
        await catalogue.RemoveArticlesContent(request.ArticleId, [request.InsideArticleId], cancellationToken);
        return Unit.Value;
    }
}