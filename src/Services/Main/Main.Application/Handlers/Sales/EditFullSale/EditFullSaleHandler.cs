using System.Data;
using Application.Common.Interfaces;
using Contracts.Sale;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Sales;
using Main.Application.Extensions;
using Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;
using Main.Application.Handlers.Balance.EditTransaction;
using Main.Application.Handlers.Sales.EditSale;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Core.Dtos.Amw.Sales;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Models;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Sales.EditFullSale;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record EditFullSaleCommand(
    IEnumerable<EditSaleContentDto> EditedContent,
    string SaleId,
    int CurrencyId,
    Guid UpdatedUserId,
    DateTime SaleDateTime,
    string? Comment,
    bool SellFromOtherStorages) : ICommand;

public class EditFullSaleHandler(IMediator mediator, ISaleRepository saleRepository, IMessageBroker messageBroker,
    IUnitOfWork unitOfWork)
    : ICommandHandler<EditFullSaleCommand>
{
    public async Task<Unit> Handle(EditFullSaleCommand request, CancellationToken cancellationToken)
    {
        var saleDateTime = request.SaleDateTime;
        var currencyId = request.CurrencyId;
        var saleId = request.SaleId;
        var userId = request.UpdatedUserId;
        var editedContent = request.EditedContent.ToList();
        var saleContentIds = editedContent.Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value).ToHashSet();

        var sale = await saleRepository.GetSaleForUpdate(saleId, true, cancellationToken)
                   ?? throw new SaleNotFoundException(saleId);
        var saleContents = (await saleRepository.GetSaleContentsForUpdate(saleId, true, cancellationToken))
            .ToDictionary(x => x.Id);
        var saleContentsDetails =
            (await saleRepository.GetSaleContentDetailsForUpdate(saleContents.Keys, true, cancellationToken)).ToList();

        var deletedSaleContents = saleContents
            .Where(x => !saleContentIds.Contains(x.Key))
            .ToDictionary();
        
        var totalSum = editedContent.GetTotalSum();

        var (contentGreaterCount, contentLessCount) =
            CalculateInventoryDeltas(editedContent, saleContents, saleContentsDetails);

        //Оставшиеся Id убираем из продажи
        contentLessCount.AddRange(GetRemovedContentDetails(deletedSaleContents, saleContentsDetails));
        //Возвращаем на склад
        if (contentLessCount.Count != 0)
        {
            var details = contentLessCount
                .Select(x => new RestoreContentItem(x.Detail.Adapt<SaleContentDetailDto>(), x.ArticleId));
            await RestoreContentToStorage(details, userId, cancellationToken);
        }

        //Добавленные значения для позиций чьи количества были увеличены ЗАБИРАЕМ СО СКЛАДА
        var takenStorageContents = new List<PrevAndNewValue<StorageContent>>();
        if (contentGreaterCount.Count != 0)
            takenStorageContents.AddRange(await RemoveContentFromStorage(contentGreaterCount, sale.MainStorageName,
                request.SellFromOtherStorages,
                userId, cancellationToken));

        //Редактируем транзакцию
        if (sale.Transaction.TransactionSum != totalSum || sale.Transaction.CurrencyId != currencyId)
            await EditTransaction(sale.TransactionId, currencyId, totalSum, saleDateTime, cancellationToken);

        await EditSale(editedContent, takenStorageContents, contentLessCount, saleId, currencyId, userId,
            saleDateTime, request.Comment, cancellationToken);

        await SubtractFromReservation(contentGreaterCount, userId, sale.BuyerId, cancellationToken);

        await messageBroker.Publish(new SaleEditedEvent(sale.Adapt<global::Contracts.Models.Sale.Sale>(),
            deletedSaleContents.Keys), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private (Dictionary<int, int> toTake, List<(SaleContentDetail Detail, int ArticleId)> toReturn)
        CalculateInventoryDeltas(
            List<EditSaleContentDto> editedContent,
            Dictionary<int, SaleContent> saleContent, List<SaleContentDetail> saleContentDetails)
    {
        var contentGreaterCount = new Dictionary<int, int>();
        var contentLessCount = new List<(SaleContentDetail, int)>();

        foreach (var content in editedContent)
            if (content.Id != null)
            {
                var existingSaleContent = saleContent[content.Id.Value];

                if (existingSaleContent.Count < content.Count)
                {
                    var diff = content.Count - existingSaleContent.Count;
                    contentGreaterCount[existingSaleContent.ArticleId] = contentGreaterCount
                        .GetValueOrDefault(existingSaleContent.ArticleId) + diff;
                }
                else if (existingSaleContent.Count > content.Count)
                {
                    var diff = existingSaleContent.Count - content.Count;
                    var detailsQueue = new Queue<SaleContentDetail>(
                        saleContentDetails.Where(x => x.SaleContentId == content.Id));

                    while (diff > 0 && detailsQueue.Count > 0)
                    {
                        var detail = detailsQueue.Dequeue();
                        var tempCount = Math.Min(detail.Count, diff);
                        var newDetail = detail.Adapt<SaleContentDetail>();
                        newDetail.Count = tempCount;
                        diff -= tempCount;
                        contentLessCount.Add((newDetail, content.ArticleId));
                    }
                }
            }
            else
            {
                contentGreaterCount[content.ArticleId] = contentGreaterCount
                    .GetValueOrDefault(content.ArticleId) + content.Count;
            }

        return (contentGreaterCount, contentLessCount);
    }

    private List<(SaleContentDetail Detail, int ArticleId)> GetRemovedContentDetails(
        Dictionary<int, SaleContent> deletedSaleContents,
        List<SaleContentDetail> saleContentDetails)
    {
        var removedDetails = new List<(SaleContentDetail, int)>();

        foreach (var (id, deletedContent) in deletedSaleContents)
        {
            var details = saleContentDetails
                .Where(x => x.SaleContentId == id);
            removedDetails.AddRange(details.Select(detail => (detail, deletedContent.ArticleId)));
        }

        return removedDetails;
    }

    private async Task RestoreContentToStorage(IEnumerable<RestoreContentItem> details,
        Guid whoUpdated, CancellationToken cancellationToken = default)
    {
        var command = new RestoreContentCommand(details, StorageMovementType.SaleEditing, whoUpdated);
        await mediator.Send(command, cancellationToken);
    }

    private async Task<IEnumerable<PrevAndNewValue<StorageContent>>> RemoveContentFromStorage(
        Dictionary<int, int> content, string? storageName, bool takeFromOtherStorages, Guid whoMoved,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveContentCommand(content, whoMoved, storageName, takeFromOtherStorages,
            StorageMovementType.SaleEditing);
        return (await mediator.Send(command, cancellationToken)).Changes;
    }

    private async Task EditTransaction(Guid transactionId, int currencyId, decimal totalSum, DateTime saleDateTime,
        CancellationToken cancellationToken = default)
    {
        var command =
            new EditTransactionCommand(transactionId, currencyId, totalSum, TransactionStatus.Sale, saleDateTime);
        await mediator.Send(command, cancellationToken);
    }

    private async Task EditSale(List<EditSaleContentDto> editedContent,
        IEnumerable<PrevAndNewValue<StorageContent>> storageContents,
        IEnumerable<(SaleContentDetail, int)> contentLessCount, string saleId, int currencyId, Guid whoUpdated,
        DateTime saleDateTime,
        string? comment, CancellationToken cancellationToken = default)
    {
        var movedToStorage = contentLessCount
            .GroupBy(x => x.Item1.SaleContentId, x => x.Item1)
            .ToDictionary(x => x.Key, x => x.ToList());
        var command = new EditSaleCommand(editedContent, storageContents, movedToStorage, saleId, currencyId,
            whoUpdated, saleDateTime, comment);
        await mediator.Send(command, cancellationToken);
    }

    private async Task SubtractFromReservation(Dictionary<int, int> graterCount, Guid whoUpdated, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new SubtractCountFromReservationsCommand(userId, whoUpdated, graterCount);
        await mediator.Send(command, cancellationToken);
    }
}