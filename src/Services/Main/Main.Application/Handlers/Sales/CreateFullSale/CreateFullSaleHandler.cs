using System.Text;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.Sales;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Core.Models;
using Core.StaticFunctions;
using Exceptions.Exceptions.Sales;
using Exceptions.Exceptions.Storages;
using Main.Application.Events;
using Main.Application.Extensions;
using Main.Application.Handlers.ArticleReservations.GetArticlesWithNotEnoughStock;
using Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Application.Handlers.StorageContents.RemoveContent;
using MediatR;
using IsolationLevel = System.Data.IsolationLevel;
using TransactionStatus = Core.Enums.TransactionStatus;

namespace Main.Application.Handlers.Sales.CreateFullSale;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record CreateFullSaleCommand(
    Guid CreatedUserId,
    Guid BuyerId,
    int CurrencyId,
    string StorageName,
    bool SellFromOtherStorages,
    DateTime SaleDateTime,
    IEnumerable<NewSaleContentDto> SaleContent,
    string? Comment,
    decimal? PayedSum,
    string? ConfirmationCode) : ICommand;

public class CreateFullSaleHandler(IMediator mediator, IArticlesRepository articlesRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateFullSaleCommand, Unit>
{
    public async Task<Unit> Handle(CreateFullSaleCommand request, CancellationToken cancellationToken)
    {
        var buyerId = request.BuyerId;
        var whoCreated = request.CreatedUserId;
        var currencyId = request.CurrencyId;
        var storageName = request.StorageName;
        var sellFromOtherStorages = request.SellFromOtherStorages;
        var saleContentList = request.SaleContent.ToList();
        var dateTimeWithoutTimeZone = DateTime.SpecifyKind(request.SaleDateTime, DateTimeKind.Unspecified);

        await CheckReservations(saleContentList, buyerId, whoCreated, storageName, sellFromOtherStorages,
            request.ConfirmationCode, cancellationToken);

        var totalSum = saleContentList.GetTotalSum();
        var transaction = await CreateTransaction(totalSum, Global.SystemId, buyerId, currencyId, whoCreated,
            dateTimeWithoutTimeZone, cancellationToken);

        var changedStorageContents = (await RemoveContentFromStorage(saleContentList,
            whoCreated, storageName, sellFromOtherStorages, cancellationToken)).ToList();

        var sale = await CreateSale(changedStorageContents, saleContentList, currencyId, buyerId, whoCreated,
            transaction.Id,
            storageName, dateTimeWithoutTimeZone, request.Comment, cancellationToken);

        if (request.PayedSum > 0)
            await CreateTransaction(request.PayedSum.Value, buyerId, Global.SystemId, currencyId, whoCreated,
                dateTimeWithoutTimeZone, cancellationToken);

        var saleCounts = sale.SaleContents
            .GroupBy(x => x.ArticleId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));

        await SubtractCountFromReservations(buyerId, whoCreated, saleCounts, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new ArticlesUpdatedEvent(saleCounts.Keys), cancellationToken);
        return Unit.Value;
    }

    private async Task SubtractCountFromReservations(Guid buyerId, Guid whoCreated, Dictionary<int, int> toSubtract,
        CancellationToken cancellation = default)
    {
        var command = new SubtractCountFromReservationsCommand(buyerId, whoCreated, toSubtract);
        await mediator.Send(command, cancellation);
    }

    private async Task CheckReservations(IEnumerable<NewSaleContentDto> saleContent, Guid buyerId,
        Guid whoCreateUserId, string storageName, bool sellFromOtherStorages, string? confirmationCode,
        CancellationToken cancellationToken = default)
    {
        var (byReservation, byStock) = await GetStockReservations(saleContent, buyerId, storageName,
            sellFromOtherStorages, cancellationToken);

        if (byStock.Count != 0)
            throw new NotEnoughCountOnStorageException(byStock.Keys);

        if (byReservation.Count != 0)
        {
            var arts = (await articlesRepository.GetArticlesByIds(byReservation.Keys, false, cancellationToken))
                .ToDictionary(x => x.Id);
            var res = new Dictionary<string, int>();
            var codeBuilder = new StringBuilder();
            codeBuilder.Append(ConcurrencyStatic.GetConcurrencyCode(whoCreateUserId));
            foreach (var (id, count) in byReservation.OrderBy(x => x.Key))
            {
                var art = arts[id];
                var key = $"{art.Producer.Name}_{art.ArticleNumber}";
                res[key] = count;
                codeBuilder.Append(ConcurrencyStatic.GetConcurrencyCode(key, count));
            }

            var currentCode = codeBuilder.ToString();
            if (currentCode != confirmationCode)
                throw new SoftConfirmationNeededException(currentCode, res);
        }
    }

    private async Task<(Dictionary<int, int> byReservation, Dictionary<int, int> byStock)> GetStockReservations(
        IEnumerable<NewSaleContentDto> saleContent, Guid buyerId, string storageName,
        bool sellFromOtherStorages, CancellationToken cancellationToken = default)
    {
        var neededArticlesCounts = saleContent
            .GroupBy(x => x.ArticleId)
            .ToDictionary(x => x.Key,
                x => x.Sum(z => z.Count));

        var result = await mediator.Send(new GetArticlesWithNotEnoughStockQuery(buyerId, storageName,
            sellFromOtherStorages, neededArticlesCounts), cancellationToken);

        return (result.NotEnoughByReservation, result.NotEnoughByStock);
    }

    private async Task<Transaction> CreateTransaction(decimal amount, Guid sender, Guid receiver,
        int currencyId, Guid createdUserId, DateTime transactionDateTime,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTransactionCommand(sender, receiver, amount, currencyId, createdUserId,
            transactionDateTime, TransactionStatus.Sale);
        var result = await mediator.Send(command, cancellationToken);
        return result.Transaction;
    }

    private async Task<IEnumerable<PrevAndNewValue<StorageContent>>> RemoveContentFromStorage(
        IEnumerable<NewSaleContentDto> saleContent,
        Guid whoCreateUserId, string storageName, bool saleFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        var dict = saleContent.GroupBy(x => x.ArticleId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));
        var command = new RemoveContentCommand(dict, whoCreateUserId, storageName, saleFromOtherStorages,
            StorageMovementType.Sale);
        var result = await mediator.Send(command, cancellationToken);
        return result.Changes;
    }

    private async Task<Sale> CreateSale(IEnumerable<PrevAndNewValue<StorageContent>> storageContents,
        IEnumerable<NewSaleContentDto> saleContent, int currencyId, Guid buyerId, Guid whoCreatedUserId,
        string transactionId, string mainStorage, DateTime saleDateTime, string? comment,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSaleCommand(saleContent, storageContents, currencyId, buyerId, whoCreatedUserId,
            transactionId, mainStorage, saleDateTime, comment);
        var result = await mediator.Send(command, cancellationToken);
        return result.Sale;
    }
}