using Core.Interface;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.EditArticle;

public record EditArticleCommand(int ArticleId, PatchArticleDto PatchArticle) : ICommand<Unit>;



public class EditArticleHandler(DContext context) : ICommandHandler<EditArticleCommand, Unit>
{
    public async Task<Unit> Handle(EditArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await context.Articles
            .FirstOrDefaultAsync(x => x.Id == request.ArticleId, cancellationToken) ?? throw new ArticleNotFoundException(request.ArticleId);
        request.PatchArticle.Adapt(article);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}