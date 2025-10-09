using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Main.Application.Events;
using Main.Application.Extensions;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record RestoreContentCommand(
    IEnumerable<RestoreContentItem> ContentDetails,
    StorageMovementType MovementType,
    Guid UserId) : ICommand;

public class RestoreContentHandler(
    ICurrencyRepository currencyRepository,
    IUserRepository usersRepository,
    IStorageContentRepository contentRepository,
    IArticlesRepository articlesRepository,
    IStoragesRepository storagesRepository,
    IArticlesService articlesService,
    IUnitOfWork unitOfWork,
    ICurrencyConverter currencyConverter,
    IMediator mediator) : ICommandHandler<RestoreContentCommand>
{
    public async Task<Unit> Handle(RestoreContentCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var contentDetailsList = request.ContentDetails.ToList();
        var articleIds = new HashSet<int>();
        var currencyIds = new HashSet<int>();
        var storageIds = new HashSet<string>();
        var contentIdStorageIdMix = new HashSet<(int, string)>();

        foreach (var (detail, articleId) in contentDetailsList)
        {
            articleIds.Add(articleId);
            currencyIds.Add(detail.CurrencyId);
            storageIds.Add(detail.Storage);
            if (detail.StorageContentId == null) continue;
            contentIdStorageIdMix.Add((detail.StorageContentId.Value, detail.Storage));
        }

        await ValidateData(userId, articleIds, currencyIds, storageIds, cancellationToken);

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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticlesUpdatedEvent(articleIds), cancellationToken);
        await mediator.Publish(new ArticlePricesUpdatedEvent(articleIds), cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(Guid userId, IEnumerable<int> articleIds, IEnumerable<int> currencyIds,
        IEnumerable<string> storageIds, CancellationToken cancellationToken = default)
    {
        await currencyRepository.EnsureCurrenciesExists(currencyIds, cancellationToken);
        await usersRepository.EnsureUsersExists([userId], cancellationToken);
        await storagesRepository.EnsureStoragesExists(storageIds, cancellationToken);
        await articlesRepository.EnsureArticlesExistForUpdate(articleIds, false, cancellationToken);
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