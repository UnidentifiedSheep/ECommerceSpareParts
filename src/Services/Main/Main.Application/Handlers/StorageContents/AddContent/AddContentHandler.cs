using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.StorageContents.AddContent;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record AddContentCommand(
    IEnumerable<NewStorageContentDto> StorageContent,
    string StorageName,
    Guid UserId,
    StorageMovementType MovementType) : ICommand;

public class AddContentHandler(
    ICurrencyRepository currencyRepository,
    IStoragesRepository storagesRepository,
    IUserRepository usersRepository,
    IArticlesRepository articlesRepository,
    IUnitOfWork unitOfWork,
    ICurrencyConverter currencyConverter,
    IArticlesService articlesService,
    IMediator mediator) : ICommandHandler<AddContentCommand>
{
    public async Task<Unit> Handle(AddContentCommand request, CancellationToken cancellationToken)
    {
        var articleIds = request.StorageContent.Select(x => x.ArticleId).ToHashSet();
        var currencyIds = request.StorageContent.Select(x => x.CurrencyId).ToHashSet();

        await ValidateData(articleIds, currencyIds, request.StorageName, request.UserId, cancellationToken);

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
        await mediator.Publish(new ArticlePricesUpdatedNotification(articleIds), cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(IEnumerable<int> articleIds, IEnumerable<int> currencyIds, string storageName,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await currencyRepository.EnsureCurrenciesExists(currencyIds, cancellationToken);
        await articlesRepository.EnsureArticlesExistForUpdate(articleIds, false, cancellationToken);
        await storagesRepository.EnsureStorageExists(storageName, cancellationToken);
        await usersRepository.EnsureUsersExists([userId], cancellationToken);
    }
}