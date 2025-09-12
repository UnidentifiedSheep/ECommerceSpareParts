using Application.Interfaces;
using Core.Attributes;
using Core.Entities;
using Core.Interfaces.Services;
using MediatR;

namespace Application.Handlers.ArticleContent.AddArticleContent;

[Transactional]
public record AddArticleContentCommand(int ArticleId, Dictionary<int, int> Content) : ICommand;

public class AddArticleContentHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddArticleContentCommand>
{
    public async Task<Unit> Handle(AddArticleContentCommand request, CancellationToken cancellationToken)
    {
        var contents = request.Content.Select(x => new ArticlesContent
        {
            InsideArticleId = x.Key,
            MainArticleId = request.ArticleId,
            Quantity = x.Value
        });
        await unitOfWork.AddRangeAsync(contents, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}