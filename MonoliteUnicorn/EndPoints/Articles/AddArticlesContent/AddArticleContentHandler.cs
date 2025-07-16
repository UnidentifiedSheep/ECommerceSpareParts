using Core.Interface;
using MediatR;
using MonoliteUnicorn.Services.Catalogue;

namespace MonoliteUnicorn.EndPoints.Articles.AddArticlesContent;

public record AddArticleContentCommand(int ArticleId, Dictionary<int, int> Content) : ICommand;

public class AddArticleContentHandler(ICatalogue catalogue) : ICommandHandler<AddArticleContentCommand>
{
    public async Task<Unit> Handle(AddArticleContentCommand request, CancellationToken cancellationToken)
    {
        await catalogue.AddArticlesContent(request.ArticleId, request.Content, cancellationToken);
        return Unit.Value;
    }
}