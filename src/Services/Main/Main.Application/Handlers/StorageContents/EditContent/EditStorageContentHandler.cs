using System.Data;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Exceptions.Base;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models;
using Main.Application.Notifications;
using Main.Entities;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.StorageContents.EditContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record EditStorageContentCommand(Dictionary<int, ModelWithCode<PatchStorageContentDto, string>> EditedFields,
    Guid UserId) : ICommand;

public class EditStorageContentHandler(
    IStorageContentService storageContentService,
    IUnitOfWork unitOfWork,
    IConcurrencyValidator<StorageContent> concurrencyValidator,
    ICurrencyConverter currencyConverter,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    IArticlesService articlesService) : ICommandHandler<EditStorageContentCommand>
{
    public async Task<Unit> Handle(EditStorageContentCommand request, CancellationToken cancellationToken)
    {
        var editedFields = request.EditedFields;

        var storageContents = await storageContentService
            .GetStorageContentsForUpdate(editedFields.Keys, cancellationToken);
        var articleIds = new HashSet<int>();
        var storageMovements = new List<StorageMovement>();
        var toIncrement = new Dictionary<int, int>();
        foreach (var item in editedFields)
        {
            var patchDto = item.Value.Model;
            var content = storageContents[item.Key];
            var clientConcurrencyCode = item.Value.Code;
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
        
        await publishEndpoint.Publish(new ArticleBuyPricesChangedEvent { ArticleIds = articleIds}, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticlesUpdatedNotification(articleIds), cancellationToken);
        return Unit.Value;
    }

    private (int diff, StorageMovement? movement) CalculateDiffAndMovement(StorageContent content,
        PatchStorageContentDto patch, Guid userId)
    {
        if (!patch.Count.IsSet || patch.Count.Value == content.Count)
            return (0, null);

        var diff = patch.Count.Value - content.Count;
        var movement = content.Adapt<StorageMovement>()
            .SetActionType(StorageMovementType.StorageContentEditing);
        movement.Count = diff;
        movement.WhoMoved = userId;

        return (diff, movement);
    }
}