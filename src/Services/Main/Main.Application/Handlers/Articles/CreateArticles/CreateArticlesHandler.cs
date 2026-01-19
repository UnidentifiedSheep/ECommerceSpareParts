using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Abstractions.Dtos.Services.Articles;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.Articles.CreateArticles;

[Transactional]
public record CreateArticlesCommand(List<CreateArticleDto> NewArticles) : ICommand<CreateArticlesResult>;
public record CreateArticlesResult(List<int> CreatedIds);

public class CreateArticlesHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateArticlesCommand, CreateArticlesResult>
{
    public async Task<CreateArticlesResult> Handle(CreateArticlesCommand request, CancellationToken cancellationToken)
    {
        var articles = request.NewArticles.Adapt<List<Article>>();
        await unitOfWork.AddRangeAsync(articles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateArticlesResult(articles.Select(x => x.Id).ToList());
    }
}