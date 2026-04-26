using System.Text;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Articles;
using Main.Abstractions.Models;
using Main.Application.Dtos.Amw.Sales;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.ProductReservations.GetArticlesWithNotEnoughStock;
using Main.Application.Handlers.ProductReservations.UpdateReservationsCounts;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Balance;
using Main.Entities.Exceptions.Sales;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Product;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;
using Utils;
using IsolationLevel = System.Data.IsolationLevel;

namespace Main.Application.Handlers.Sales.CreateFullSale;

[AutoSave]
[Transactional(IsolationLevel.Serializable, 20, 2)]
public record CreateFullSaleCommand(
    Guid BuyerId,
    int CurrencyId,
    string StorageName,
    bool SellFromOtherStorages,
    DateTime SaleDateTime,
    IEnumerable<NewSaleContentDto> SaleContent,
    string? Comment,
    decimal? PayedSum,
    string? ConfirmationCode) : ICommand;

public class CreateFullSaleHandler(
    ISender sender,
    IProductRepository productRepository,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<CreateFullSaleCommand, Unit>
{
    public async Task<Unit> Handle(CreateFullSaleCommand request, CancellationToken cancellationToken)
    {
        var buyerId = request.BuyerId;
        var currencyId = request.CurrencyId;
        var storageName = request.StorageName;
        var sellFromOtherStorages = request.SellFromOtherStorages;
        var saleContentList = request.SaleContent.ToList();
        var dateTime = request.SaleDateTime;

        await CheckReservations(saleContentList, buyerId, storageName, sellFromOtherStorages,
            request.ConfirmationCode, cancellationToken);

        var transaction = await CreateTransaction(
            amount: saleContentList.GetTotalSum(), 
            senderId: Global.SystemId, 
            receiverId: buyerId, 
            currencyId: currencyId, 
            transactionDateTime: dateTime, 
            cancellationToken: cancellationToken);

        var changedStorageContents = (await RemoveContentFromStorage(saleContentList, 
            storageName, sellFromOtherStorages, cancellationToken)).ToList();

        var sale = await CreateSale(changedStorageContents, saleContentList, currencyId, buyerId,
            transaction.Id,
            storageName, dateTime, request.Comment, cancellationToken);

        if (request.PayedSum > 0)
            await CreateTransaction(
                amount: request.PayedSum.Value, 
                senderId: buyerId, 
                receiverId: Global.SystemId, 
                currencyId: currencyId, 
                transactionDateTime: dateTime, 
                cancellationToken: cancellationToken);

        var saleCounts = sale.Contents
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));

        await SubtractCountFromReservations(buyerId, saleCounts, cancellationToken);
        
        foreach (var productId in saleCounts.Keys)
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = productId
            });
        
        return Unit.Value;
    }

    private async Task SubtractCountFromReservations(
        Guid buyerId,
        Dictionary<int, int> toSubtract,
        CancellationToken cancellation = default)
    {
        var command = new UpdateReservationsCountsCommand(buyerId, toSubtract);
        await sender.Send(command, cancellation);
    }

    private async Task CheckReservations(
        IEnumerable<NewSaleContentDto> saleContent,
        Guid buyerId,
        string storageName,
        bool sellFromOtherStorages,
        string? confirmationCode,
        CancellationToken cancellationToken = default)
    {
        var (byReservation, byStock) = await GetStockReservations(saleContent, buyerId, storageName,
            sellFromOtherStorages, cancellationToken);

        if (byStock.Count != 0)
            throw new NotEnoughCountOnStorageException(byStock.Keys);

        if (byReservation.Count != 0)
        {
            var criteria = Criteria<Product>.New()
                .Track(false)
                .Where(x => byReservation.Keys.Contains(x.Id))
                .Build();
            
            var products = (await productRepository.ListAsync(criteria, cancellationToken))
                .ToDictionary(x => x.Id);
            
            var res = new Dictionary<string, int>();
            var codeBuilder = new StringBuilder();
            foreach (var (id, count) in byReservation.OrderBy(x => x.Key))
            {
                var art = products[id];
                var key = $"{art.Producer.Name}_{art.Sku.NormalizedValue}";
                res[key] = count;
                codeBuilder.Append(HashUtils.ComputeHash(key, count));
            }

            var currentCode = codeBuilder.ToString();
            if (currentCode != confirmationCode)
                throw new SaleSoftConfirmationNeededException(currentCode, res);
        }
    }

    private async Task<(Dictionary<int, int> byReservation, Dictionary<int, int> byStock)> GetStockReservations(
        IEnumerable<NewSaleContentDto> saleContent,
        Guid buyerId,
        string storageName,
        bool sellFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        var neededArticlesCounts = saleContent
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key,
                x => x.Sum(z => z.Count));

        var result = await sender.Send(new GetArticlesWithNotEnoughStockQuery(buyerId, storageName,
            sellFromOtherStorages, neededArticlesCounts), cancellationToken);

        return (result.NotEnoughByReservation, result.NotEnoughByStock);
    }

    private async Task<Transaction> CreateTransaction(
        decimal amount,
        Guid senderId,
        Guid receiverId,
        int currencyId,
        DateTime transactionDateTime,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTransactionCommand(senderId, receiverId, amount, currencyId, transactionDateTime);
        var result = await sender.Send(command, cancellationToken);
        return result.Transaction;
    }

    private async Task<IEnumerable<PrevAndNewValue<StorageContent>>> RemoveContentFromStorage(
        IEnumerable<NewSaleContentDto> saleContent,
        string storageName,
        bool saleFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        var dict = saleContent.GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));
        var command = new RemoveContentCommand(dict, storageName, saleFromOtherStorages, StorageMovementType.Sale);
        var result = await sender.Send(command, cancellationToken);
        return result.Changes;
    }

    private async Task<Sale> CreateSale(
        IEnumerable<PrevAndNewValue<StorageContent>> storageContents,
        IEnumerable<NewSaleContentDto> saleContent,
        int currencyId,
        Guid buyerId,
        Guid transactionId,
        string mainStorage,
        DateTime saleDateTime,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSaleCommand(saleContent, storageContents, currencyId, buyerId,
            transactionId, mainStorage, saleDateTime, comment);
        var result = await sender.Send(command, cancellationToken);
        return result.Sale;
    }
}