using Core.Interface;
using Core.StaticFunctions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Inventory;

namespace MonoliteUnicorn.EndPoints.Storages.DeleteStorageContent;

public record DeleteStorageContentCommand(int ContentId, string ConcurrencyCode, string UserId) : ICommand;

public class DeleteStorageContentHandler(DContext context, IInventory inventoryService) : ICommandHandler<DeleteStorageContentCommand>
{
    public async Task<Unit> Handle(DeleteStorageContentCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        
        var content = await context.StorageContents
                          .FromSql($"select * from storage_content where id = {request.ContentId} for update")
                          .FirstOrDefaultAsync(cancellationToken) ?? throw new StorageContentNotFoundException(request.ContentId);
        
        var currConcurrencyCode = ConcurrencyStatic.GetConcurrencyCode(content.Id, content.ArticleId,
            content.BuyPrice, content.CurrencyId, content.StorageName, 
            content.BuyPriceInUsd, content.Count, content.PurchaseDatetime);

        if (currConcurrencyCode != request.ConcurrencyCode)
            throw new ConcurrencyCodeMismatchException(request.ConcurrencyCode, currConcurrencyCode);

        await inventoryService.DeleteContentFromStorage(content.Id, request.UserId,
            StorageMovementType.StorageContentDeletion, cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}