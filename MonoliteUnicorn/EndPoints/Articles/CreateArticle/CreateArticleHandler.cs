using Core.Interface;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.CreateArticle;

public record CreateArticleCommand(List<NewArticleDto> NewArticles) : ICommand;
public class CreateArticleHandler(DContext context) : ICommandHandler<CreateArticleCommand>
{
    public async Task<Unit> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        var articles = request.NewArticles.Adapt<List<Article>>();
        var producersIds = articles
            .Select(x => x.ProducerId)
            .Distinct()
            .ToList();
        var producers = await context.Producers
            .AsNoTracking()
            .Where(x => producersIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, 
                cancellationToken: cancellationToken);
        if (producers.Count != producersIds.Count)
        {
            var notFoundIds = producersIds.Except(producers.Keys);
            throw new ProducerNotFoundException(notFoundIds);
        }
        await context.Articles.AddRangeAsync(articles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}