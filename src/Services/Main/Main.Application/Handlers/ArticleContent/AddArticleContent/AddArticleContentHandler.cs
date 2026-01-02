using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using MediatR;

namespace Main.Application.Handlers.ArticleContent.AddArticleContent;

[Transactional]
public record AddArticleContentCommand(int ArticleId, Dictionary<int, int> Content) : ICommand;


public class AddArticleContentHandler(IUnitOfWork unitOfWork, DbDataValidatorBase dbValidator) : ICommandHandler<AddArticleContentCommand>
{
    public async Task<Unit> Handle(AddArticleContentCommand request, CancellationToken cancellationToken)
    {
        
        var contents = request.Content.Select(x => new ArticlesContent
        {
            InsideArticleId = x.Key,
            MainArticleId = request.ArticleId,
            Quantity = x.Value
        }).ToList();

        var ids = contents.Select(x => x.InsideArticleId).ToHashSet();
        ids.Add(request.ArticleId);
        var plan = new ValidationPlan().EnsureArticleExists(ids);
        await dbValidator.Validate(plan, true, true, cancellationToken);
        
        await unitOfWork.AddRangeAsync(contents, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}