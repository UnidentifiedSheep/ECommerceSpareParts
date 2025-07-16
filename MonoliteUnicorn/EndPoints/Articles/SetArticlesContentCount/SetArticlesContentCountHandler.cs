using Core.Interface;
using MediatR;
using MonoliteUnicorn.Services.Catalogue;

namespace MonoliteUnicorn.EndPoints.Articles.SetArticlesContentCount;

public record SetArticlesContentCountCommand(int ArticleId, int InsideArticleId, int Count) : ICommand;

public class SetArticlesContentCountHandler(ICatalogue catalogue) : ICommandHandler<SetArticlesContentCountCommand>
{
    public async Task<Unit> Handle(SetArticlesContentCountCommand request, CancellationToken cancellationToken)
    {
        await catalogue.SetArticleContentCount(request.ArticleId, request.InsideArticleId, request.Count, cancellationToken);
        return Unit.Value;
    }
}