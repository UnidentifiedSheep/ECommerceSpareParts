using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Contracts.Models.Articles;
using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Producers;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Notifications;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Articles.PatchArticle;

[Transactional]
public record PatchArticleCommand(int ArticleId, PatchArticleDto PatchArticle) : ICommand;

public class PatchArticleHandler(IMediator mediator, IArticlesRepository articlesRepository, IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint, IProducerRepository producerRepository)
    : ICommandHandler<PatchArticleCommand>
{
    public async Task<Unit> Handle(PatchArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await articlesRepository.GetArticleById(request.ArticleId, true, cancellationToken)
                      ?? throw new ArticleNotFoundException(request.ArticleId);

        request.PatchArticle.Adapt(article);
        var producer = await producerRepository.GetProducer(article.ProducerId, false, cancellationToken)
                          ?? throw new ProducerNotFoundException(article.ProducerId);

        var adaptedArticle = article.Adapt<Article>() with
        {
            ProducerId = article.ProducerId, 
            ProducerName =  producer.Name
        };
        await publishEndpoint.Publish(new ArticleUpdatedEvent { Article = adaptedArticle }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new ArticleUpdatedNotification(request.ArticleId), cancellationToken);

        return Unit.Value;
    }
}