using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services.Storage;
using Main.Application.Models;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

[AutoSave]
[Transactional(
    IsolationLevel.ReadCommitted,
    20,
    2)]
public record RestoreContentCommand(
    IEnumerable<RestoreContentItem> ContentDetails,
    StorageMovementType MovementType
) : ICommand;

public class RestoreContentHandler(
    IStorageContentRepository contentRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IStorageContentChangeNotifier changeNotifier
) : ICommandHandler<RestoreContentCommand>
{
    public async Task<Unit> Handle(RestoreContentCommand request, CancellationToken cancellationToken)
    {
        var contentDetailsList = request.ContentDetails.ToList();
        var productIds = new HashSet<int>();
        var storageContentIds = new HashSet<int>();

        foreach (var detail in contentDetailsList)
        {
            productIds.Add(detail.ProductId);
            storageContentIds.Add(detail.StorageContentId);
        }

        var products = await productRepository
            .EnsureExistsForUpdateAsync(
                productIds,
                nf => new ProductNotFoundException(nf),
                cancellationToken);

        var storageContents = await contentRepository
            .EnsureExistsForUpdateAsync(
                storageContentIds,
                nf => new StorageContentNotFoundException(nf),
                cancellationToken);

        var events = new List<Event>();

        foreach (var detail in contentDetailsList)
        {
            var product = products[detail.ProductId];
            var content = storageContents[detail.StorageContentId];

            events.Add(
                StorageMovementEvent.Create(
                    new StorageMovementEventData
                    {
                        ProductId = content.ProductId,
                        StorageName = content.StorageName,
                        CurrencyId = content.CurrencyId,
                        Count = detail.Count,
                        BuyPrice = content.BuyPrice,
                        MovementType = request.MovementType
                    }));

            content.IncreaseCount(detail.Count);
            product.IncreaseStock(detail.Count);
        }

        await unitOfWork.AddRangeAsync(events, cancellationToken);

        changeNotifier.NotifyChanged(productIds);

        return Unit.Value;
    }
}