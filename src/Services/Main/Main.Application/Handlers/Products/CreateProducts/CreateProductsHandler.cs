using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Main.Abstractions.Dtos.Services.Articles;
using Main.Abstractions.Exceptions.Producers;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Mapster;
using MassTransit;
using ContractArticle = Contracts.Models.Articles.Article;

namespace Main.Application.Handlers.Articles.CreateArticles;

[AutoSave]
[Transactional]
public record CreateProductsCommand(List<CreateArticleDto> NewArticles) : ICommand<CreateProductsResult>;

public record CreateProductsResult(List<int> CreatedIds);

public class CreateProductsHandler(
    IUnitOfWork unitOfWork,
    IProducerRepository producerRepository,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreateProductsCommand, CreateProductsResult>
{
    public async Task<CreateProductsResult> Handle(CreateProductsCommand request, CancellationToken cancellationToken)
    {
        var products = new List<Product>();

        foreach (var @new in request.NewArticles)
        {
            var product = Product.Create(@new.Sku, @new.Name, @new.ProducerId, @new.Description);
            product.SetIndicator(@new.Indicator);
            product.SetCategory(@new.CategoryId);
            products.Add(product);
        }
        
        await unitOfWork.AddRangeAsync(products, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await PublishEvent(products, cancellationToken);

        return new CreateProductsResult(products.Select(x => x.Id).ToList());
    }

    private async Task PublishEvent(List<Product> articles, CancellationToken cancellationToken)
    {
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
            };
            adaptedArticles.Add(adaptedArticle);
        }

        await publishEndpoint.Publish(new ArticlesCreatedEvent
        {
            Articles = adaptedArticles
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}