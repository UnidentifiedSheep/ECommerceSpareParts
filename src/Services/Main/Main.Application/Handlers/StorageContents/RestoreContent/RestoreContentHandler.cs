using System.Data;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Entities;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record RestoreContentCommand(IEnumerable<RestoreContentItem> ContentDetails, StorageMovementType MovementType,
    Guid UserId) : ICommand;

public class RestoreContentHandler(IStorageContentRepository contentRepository, IArticlesRepository articlesRepository,
    IArticlesService articlesService, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter, 
    IPublishEndpoint publishEndpoint, IMediator mediator) : ICommandHandler<RestoreContentCommand>
{
    public async Task<Unit> Handle(RestoreContentCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var contentDetailsList = request.ContentDetails.ToList();
        var articleIds = new HashSet<int>();
        var contentIdStorageIdMix = new HashSet<(int, string)>();

        foreach (var (detail, articleId) in contentDetailsList)
        {
            articleIds.Add(articleId);
            if (detail.StorageContentId == null) continue;
            contentIdStorageIdMix.Add((detail.StorageContentId.Value, detail.Storage));
        }

        await articlesRepository.EnsureArticlesExistForUpdate(articleIds, false, cancellationToken);

        var toIncrement = new Dictionary<int, int>();
        var storageContents = await contentRepository
            .GetStorageContentsForUpdateAsync(contentIdStorageIdMix, true, cancellationToken);

        foreach (var (detail, articleId) in contentDetailsList)
        {
            StorageContent? content = null;
            if (detail.StorageContentId != null)
                content = storageContents.GetValueOrDefault((detail.StorageContentId.Value, detail.Storage));
            if (content != null)
            {
                content.Count += detail.Count;
            }
            else
            {
                content = detail.Adapt<StorageContent>();
                content.BuyPriceInUsd = currencyConverter.ConvertToUsd(content.BuyPrice, content.CurrencyId);
                content.ArticleId = articleId;
                await unitOfWork.AddAsync(content, cancellationToken);
            }

            await AddMovement(content, userId, detail.Count, request.MovementType, cancellationToken);
            toIncrement[articleId] = toIncrement.GetValueOrDefault(articleId) + detail.Count;
        }

        await articlesService.UpdateArticlesCount(toIncrement, cancellationToken);
        
        await publishEndpoint.Publish(new ArticleBuyPricesChangedEvent { ArticleIds = articleIds}, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticlesUpdatedNotification(articleIds), cancellationToken);
        return Unit.Value;
    }

    private async Task AddMovement(StorageContent content, Guid userId, int movementCount,
        StorageMovementType movementType,
        CancellationToken cancellationToken = default)
    {
        var tempMovement = content.Adapt<StorageMovement>()
            .SetActionType(movementType);
        tempMovement.Count = movementCount;
        tempMovement.WhoMoved = userId;
        await unitOfWork.AddAsync(tempMovement, cancellationToken);
    }
}