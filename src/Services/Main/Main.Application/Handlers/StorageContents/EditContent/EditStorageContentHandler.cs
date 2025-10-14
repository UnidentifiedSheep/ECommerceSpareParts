using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Base;
using Exceptions.Exceptions.Storages;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.EditContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record EditStorageContentCommand(
    Dictionary<int, (PatchStorageContentDto value, string concurrencyCode)> EditedFields,
    Guid UserId) : ICommand;

public class EditStorageContentHandler(
    DbDataValidatorBase dbValidator,
    IStorageContentRepository storageContentRepository,
    IUnitOfWork unitOfWork,
    IConcurrencyValidator<StorageContent> concurrencyValidator,
    ICurrencyConverter currencyConverter,
    IMediator mediator,
    IArticlesService articlesService) : ICommandHandler<EditStorageContentCommand>
{
    public async Task<Unit> Handle(EditStorageContentCommand request, CancellationToken cancellationToken)
    {
        var editedFields = request.EditedFields;

        await ValidateData(editedFields.Select(x => x.Value.value), request.UserId, cancellationToken);

        var storageContents = await GetAndValidateStorageContents(editedFields.Keys, cancellationToken);
        var articleIds = new HashSet<int>();
        var storageMovements = new List<StorageMovement>();
        var toIncrement = new Dictionary<int, int>();
        foreach (var item in editedFields)
        {
            var patchDto = item.Value.value;
            var content = storageContents[item.Key];
            var clientConcurrencyCode = item.Value.concurrencyCode;
            if (!concurrencyValidator.IsValid(content, clientConcurrencyCode, out var validCode))
                throw new ConcurrencyCodeMismatchException(clientConcurrencyCode, validCode);

            var (diff, storageMovement) = CalculateDiffAndMovement(content, patchDto, request.UserId);
            if (storageMovement != null)
            {
                storageMovements.Add(storageMovement);
                toIncrement[content.ArticleId] = toIncrement.GetValueOrDefault(content.ArticleId) + diff;
            }

            patchDto.Adapt(content);
            if (patchDto.CurrencyId.IsSet || patchDto.BuyPrice.IsSet)
                content.BuyPriceInUsd = currencyConverter.ConvertToUsd(content.BuyPrice, content.CurrencyId);

            articleIds.Add(content.ArticleId);
        }

        if (toIncrement.Count > 0)
            await articlesService.UpdateArticlesCount(toIncrement, cancellationToken);
        await unitOfWork.AddRangeAsync(storageMovements, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticlesUpdatedNotification(articleIds), cancellationToken);
        await mediator.Publish(new ArticlePricesUpdatedNotification(articleIds), cancellationToken);
        return Unit.Value;
    }

    private (int diff, StorageMovement? movement) CalculateDiffAndMovement(StorageContent content,
        PatchStorageContentDto patch, Guid userId)
    {
        if (!patch.Count.IsSet || patch.Count.Value == content.Count)
            return (0, null);

        var diff = patch.Count.Value - content.Count; // положительное — увеличение, отрицательное — уменьшение
        var movement = content.Adapt<StorageMovement>()
            .SetActionType(StorageMovementType.StorageContentEditing);
        movement.Count = diff;
        movement.WhoMoved = userId;

        return (diff, movement);
    }

    private async Task<Dictionary<int, StorageContent>> GetAndValidateStorageContents(
        IEnumerable<int> storageContentIds, CancellationToken cancellationToken)
    {
        var ids = storageContentIds.ToHashSet();
        var storageContents = (await storageContentRepository.GetStorageContentsForUpdate(ids,
            true, cancellationToken)).ToDictionary(x => x.Id);
        if (storageContents.Count != ids.Count)
            throw new StorageContentNotFoundException(ids.Except(storageContents.Keys));
        return storageContents;
    }

    private async Task ValidateData(IEnumerable<PatchStorageContentDto> values, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var currencyIds = new HashSet<int>();
        var storageIds = new HashSet<string>();

        foreach (var value in values)
        {
            if (value.StorageName.IsSet)
                storageIds.Add(value.StorageName.Value!);
            if (value.CurrencyId.IsSet)
                currencyIds.Add(value.CurrencyId.Value);
        }

        var plan = new ValidationPlan()
            .EnsureUserExists(userId)
            .EnsureCurrencyExists(currencyIds)
            .EnsureStorageExists(storageIds);
        
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}