using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.Producers;
using Main.Abstractions.Dtos.Services.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;
using MassTransit;

using ContractArticle = Contracts.Models.Articles.Article;

namespace Main.Application.Handlers.Articles.CreateArticles;

[Transactional]
public record CreateArticlesCommand(List<CreateArticleDto> NewArticles) : ICommand<CreateArticlesResult>;
public record CreateArticlesResult(List<int> CreatedIds);

public class CreateArticlesHandler(IUnitOfWork unitOfWork, IProducerRepository producerRepository, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreateArticlesCommand, CreateArticlesResult>
{
    public async Task<CreateArticlesResult> Handle(CreateArticlesCommand request, CancellationToken cancellationToken)
    {
        var articles = request.NewArticles.Adapt<List<Article>>();
        await unitOfWork.AddRangeAsync(articles, cancellationToken);
        var producerIds = articles.Select(x => x.ProducerId).Distinct();
        var producers = (await producerRepository
            .GetProducers(producerIds, false, cancellationToken))
            .ToDictionary(k => k.Id, v => v);
        
        var adaptedArticles = new List<ContractArticle>();
        foreach (var article in articles)
        {
            if (!producers.TryGetValue(article.ProducerId, out var producer))
                throw new ProducerNotFoundException(article.ProducerId);
            var adaptedArticle = article.Adapt<ContractArticle>() with
            {
                ProducerId = producer.Id,
                ProducerName = producer.Name
            };
            adaptedArticles.Add(adaptedArticle);
        }
        
        await  publishEndpoint.Publish(adaptedArticles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateArticlesResult(articles.Select(x => x.Id).ToList());
    }
}