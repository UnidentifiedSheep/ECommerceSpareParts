using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Articles;
using Domain.Extensions;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Models;
using Main.Application.Extensions;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Event;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.EditContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record EditStorageContentCommand(
    Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>> EditedFields) : ICommand;

public class EditStorageContentHandler(
    IRepository<StorageContent, int> storageContentRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope
    ) : ICommandHandler<EditStorageContentCommand>
{
    public async Task<Unit> Handle(EditStorageContentCommand request, CancellationToken cancellationToken)
    {
        var editedFields = request.EditedFields;
        
        var storageContents = await storageContentRepository
            .EnsureStorageContentsExistsForUpdateAsync(editedFields.Keys, cancellationToken);

        var products = await productRepository
            .EnsureProductsExistsForUpdateAsync(
                productIds: storageContents.Select(x => x.Value.ProductId), 
                cancellationToken: cancellationToken);
        
        var storageMovements = new List<Event>();
        
        foreach (var item in editedFields)
        {
            PatchStorageContentDto patch = item.Value.Model;
            StorageContent content = storageContents[item.Key];
            Product product = products[item.Key];
            
            content.ValidateVersion(item.Value.RowVersion);
            product.IncreaseStock(CalculateDiff(content, patch));
            
            var movementEvent = StorageMovementEvent.Create(content, StorageMovementType.StorageContentEditing);

            patch.Count.Apply(content.SetCount);
            patch.BuyPrice.Apply(content.SetBuyPrice);
            patch.CurrencyId.Apply(content.SetCurrencyId);
            patch.PurchaseDatetime.Apply(content.SetPurchaseDate);
            
            storageMovements.Add(movementEvent);
        }
        await unitOfWork.AddRangeAsync(storageMovements, cancellationToken);

        foreach (var productId in products.Keys)
        {
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = productId
            });
            
            integrationEventScope.Add(new ProductBuyPricesUpdatedEvent
            {
                ProductId = productId
            });
        }
        
        return Unit.Value;
    }

    private int CalculateDiff(
        StorageContent content,
        PatchStorageContentDto patch)
    {
        if (!patch.Count.IsSet || patch.Count.Value == content.Count) return 0;
        return patch.Count.Value - content.Count;
    }
}