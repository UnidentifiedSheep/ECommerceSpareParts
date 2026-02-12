using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using MediatR;

namespace Main.Application.Handlers.ArticleContent.AddArticleContent;

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
        }).ToList();
        
        await unitOfWork.AddRangeAsync(contents, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}