using Core.Interface;
using MediatR;
using MonoliteUnicorn.Services.Catalogue;

namespace MonoliteUnicorn.EndPoints.Articles.SetArticleIndicator;

public record SetArticleIndicatorCommand(int ArticleId, string? Indicator) : ICommand;

public class SetArticleIndicatorHandler(ICatalogue catalogue) : ICommandHandler<SetArticleIndicatorCommand>
{
    public async Task<Unit> Handle(SetArticleIndicatorCommand request, CancellationToken cancellationToken)
    {
        await catalogue.SetArticleIndicator(request.ArticleId, request.Indicator, cancellationToken);
        return Unit.Value;
    }
}