using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Storages;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Services;
using Main.Core.Models;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.RemoveContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record RemoveContentCommand(
    Dictionary<int, int> Content,
    Guid UserId,
    string? StorageName,
    bool TakeFromOtherStorages,
    StorageMovementType MovementType) : ICommand<RemoveContentResult>;

public record RemoveContentResult(IEnumerable<PrevAndNewValue<StorageContent>> Changes);

public class RemoveContentHandler(
    DbDataValidatorBase dbValidator,
    IStorageContentRepository contentRepository,
    IArticlesRepository articlesRepository,
    IArticlesService articlesService,
    IUnitOfWork unitOfWork,
    IMediator mediator) : ICommandHandler<RemoveContentCommand, RemoveContentResult>
{
    public async Task<RemoveContentResult> Handle(RemoveContentCommand request, CancellationToken cancellationToken)
    {
        var content = request.Content;
        var takeFromOtherStorages = request.TakeFromOtherStorages;
        var userId = request.UserId;
        var storageName = request.StorageName;
        var articleIds = content.Keys;

        await ValidateData(takeFromOtherStorages, storageName, articleIds, userId, cancellationToken);

        var movements = new List<StorageMovement>();
        var toIncrement = new Dictionary<int, int>();
        var result = new List<PrevAndNewValue<StorageContent>>();

        foreach (var (articleId, count) in content)
        {
            List<StorageContent> storageContents = [];

            var availableCount = 0;
            if (!string.IsNullOrWhiteSpace(storageName))
                await foreach (var dbItem in contentRepository
                                   .GetStorageContentsForUpdateAsync(articleId, storageName)
                                   .WithCancellation(cancellationToken))
                {
                    storageContents.Add(dbItem);
                    availableCount += dbItem.Count;
                    if (availableCount >= count) break;
                }

            if (takeFromOtherStorages && availableCount < count)
                await foreach (var dbItem in contentRepository
                                   .GetStorageContentsForUpdateAsync(articleId, null, null,
                                       string.IsNullOrWhiteSpace(storageName) ? null : [storageName])
                                   .WithCancellation(cancellationToken))
                {
                    storageContents.Add(dbItem);
                    availableCount += dbItem.Count;
                    if (availableCount >= count) break;
                }

            if (availableCount < count) throw new NotEnoughCountOnStorageException(articleId, availableCount);
            var counter = count;

            foreach (var item in storageContents)
            {
                var prevValue = item.Adapt<StorageContent>();
                var temp = Math.Min(counter, item.Count);
                item.Count -= temp;
                counter -= temp;
                var newValue = item.Adapt<StorageContent>();

                var movement = GetMovement(item, request.MovementType, userId, -temp);
                movements.Add(movement);

                result.Add(new PrevAndNewValue<StorageContent>(prevValue, newValue));
                if (counter <= 0) break;
            }

            toIncrement[articleId] = -count;
        }

        await unitOfWork.AddRangeAsync(movements, cancellationToken);
        await articlesService.UpdateArticlesCount(toIncrement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticlesUpdatedNotification(articleIds), cancellationToken);
        await mediator.Publish(new ArticlePricesUpdatedNotification(articleIds), cancellationToken);

        return new RemoveContentResult(result);
    }

    private async Task ValidateData(bool takeFromOtherStorages, string? storageName, IEnumerable<int> articleIds,
        Guid userId, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan()
            .EnsureUserExists(userId);
        if (!takeFromOtherStorages && !string.IsNullOrWhiteSpace(storageName))
            plan.EnsureStorageExists(storageName);
        
        await dbValidator.Validate(plan, true, true, cancellationToken);
        await articlesRepository.EnsureArticlesExistForUpdate(articleIds, false, cancellationToken);
    }

    private StorageMovement GetMovement(StorageContent content, StorageMovementType movementType, Guid whoMoved,
        int count)
    {
        var tempMovement = content.Adapt<StorageMovement>().SetActionType(movementType);
        tempMovement.Count = count;
        tempMovement.WhoMoved = whoMoved;
        return tempMovement;
    }
}