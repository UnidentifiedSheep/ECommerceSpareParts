using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Entities;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.AddContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record AddContentCommand(IEnumerable<NewStorageContentDto> StorageContent, string StorageName,
    Guid UserId, StorageMovementType MovementType, bool RecalcPrices = true) : ICommand<AddContentResult>;

public record AddContentResult(List<StorageContentDto> StorageContents);

public class AddContentHandler(IArticlesRepository articlesRepository, IUnitOfWork unitOfWork,
    ICurrencyConverter currencyConverter, IArticlesService articlesService,
    IMediator mediator) : ICommandHandler<AddContentCommand, AddContentResult>
{
    public async Task<AddContentResult> Handle(AddContentCommand request, CancellationToken cancellationToken)
    {
        var articleIds = request.StorageContent.Select(x => x.ArticleId).ToHashSet();

        await articlesRepository.EnsureArticlesExistForUpdate(articleIds, false, cancellationToken);

        var toIncrement = new Dictionary<int, int>();
        var storageContents = new List<StorageContent>();
        var storageMovements = new List<StorageMovement>();

        foreach (var item in request.StorageContent)
        {
            var content = item.Adapt<StorageContent>();
            content.BuyPriceInUsd = currencyConverter.ConvertToUsd(item.BuyPrice, item.CurrencyId);
            content.StorageName = request.StorageName.Trim();
            storageContents.Add(content);

            var storageMovement = content
                .Adapt<StorageMovement>()
                .SetActionType(request.MovementType);
            storageMovement.WhoMoved = request.UserId;
            storageMovements.Add(storageMovement);

            toIncrement[item.ArticleId] = toIncrement.GetValueOrDefault(item.ArticleId) + item.Count;
        }

        await unitOfWork.AddRangeAsync(storageMovements, cancellationToken);
        await unitOfWork.AddRangeAsync(storageContents, cancellationToken);
        await articlesService.UpdateArticlesCount(toIncrement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ArticlesUpdatedNotification(articleIds), cancellationToken);
        if (request.RecalcPrices)
            await mediator.Publish(new ArticlePricesUpdatedNotification(articleIds), cancellationToken);
        
        return new AddContentResult(storageContents.Adapt<List<StorageContentDto>>());
    }
}