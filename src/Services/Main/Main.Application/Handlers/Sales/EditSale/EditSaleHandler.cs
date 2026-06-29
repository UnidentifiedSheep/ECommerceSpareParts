using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Models.Options;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Sale;
using Domain.Extensions;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.Event;
using Main.Entities.Balance;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Sales.EditSale;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record EditSaleCommand(
    Guid SaleId,
    uint RowVersion,
    IEnumerable<EditSaleContentDto> Content,
    int CurrencyId,
    DateTime SaleDateTime,
    string? Comment,
    string? ConfirmationCode,
    bool ForcePayment = false) : ICommand;

public class EditSaleHandler(
    ISender sender,
    ISaleRepository saleRepository,
    IOptions<SystemOptions> systemOptions,
    ISaleService saleService,
    ISaleEventService saleEventService,
    IUnitOfWork unitOfWork) : ICommandHandler<EditSaleCommand>
{
    public async Task<Unit> Handle(EditSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetFullSaleForUpdate(request.SaleId, cancellationToken)
                   ?? throw new SaleNotFoundException(request.SaleId);

        if (sale.State == SaleState.Deleted)
            throw new SaleNotFoundException(request.SaleId);

        sale.ValidateVersion(request.RowVersion);

        var contentDtos = request.Content.ToList();
        var oldCounts = sale.Contents
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));

        await saleService.RestoreContents(sale, StorageMovementType.SaleEditing, cancellationToken);

        await saleService.CheckReservations(
            contentDtos,
            sale.BuyerId,
            sale.StorageName,
            false,
            request.ConfirmationCode,
            cancellationToken);

        await ReverseSaleTransaction(sale, cancellationToken);

        var transaction = await CreateSaleTransaction(
            sale.BuyerId,
            contentDtos.Sum(x => x.PriceWithDiscount * x.Count),
            request.CurrencyId,
            request.SaleDateTime,
            request.ForcePayment,
            cancellationToken);

        sale.SetTransactionId(transaction.Id);
        sale.SetCurrency(request.CurrencyId);
        sale.SetDateTime(request.SaleDateTime);
        sale.SetComment(request.Comment);

        await UpdateContents(sale, contentDtos, false, cancellationToken);
        await UpdateReservationsCounts(sale, oldCounts, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await saleEventService.NotifyUpdated(sale.Id, cancellationToken);

        return Unit.Value;
    }

    private async Task ReverseSaleTransaction(
        Sale sale,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ReverseTransactionCommand(
                sale.TransactionId,
                TransactionReversalMode.System, 
                true),
            cancellationToken);
    }

    private async Task<Transaction> CreateSaleTransaction(
        Guid buyerId,
        decimal amount,
        int currencyId,
        DateTime transactionDateTime,
        bool forcePayment,
        CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new CreateTransactionCommand(
                    systemOptions.Value.SystemId,
                    buyerId,
                    amount,
                    currencyId,
                    transactionDateTime,
                    TransactionSourceType.Sale,
                    TransactionCreationMode.System,
                    forcePayment),
                cancellationToken))
            .Transaction;
    }

    private async Task UpdateContents(
        Sale sale,
        IReadOnlyList<EditSaleContentDto> contentDtos,
        bool sellFromOtherStorages,
        CancellationToken cancellationToken)
    {
        var existingById = sale.Contents.ToDictionary(x => x.Id);
        var requestedIds = contentDtos
            .Where(x => x.Id is not null)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        requestedIds.EnsureAllExists(
            existingById.Keys,
            ids => new SaleContentNotFoundException(ids[0]));

        var distributed = await saleService.TakeFromStorageAndDistributeDetails(
            sale.StorageName,
            contentDtos,
            StorageMovementType.SaleEditing,
            sellFromOtherStorages,
            cancellationToken);

        foreach (var removed in sale.Contents.Where(x => !requestedIds.Contains(x.Id)).ToList())
            RemoveContent(sale, removed);

        for (var i = 0; i < contentDtos.Count; i++)
        {
            var dto = contentDtos[i];
            var newContent = distributed[i];

            if (dto.Id is null)
            {
                sale.AddContent(newContent);
                continue;
            }

            UpdateContent(existingById[dto.Id.Value], dto, newContent);
        }
    }

    private void UpdateContent(
        SaleContent content,
        EditSaleContentDto dto,
        SaleContent newContent)
    {
        if (content.ProductId != dto.ProductId)
            throw new ArticleDoesntMatchContentException(content.Id);

        unitOfWork.RemoveRange(content.Details.ToList());
        content.SetPriceAndDetails(
            dto.Price,
            dto.PriceWithDiscount,
            newContent.Details);
        content.SetComment(dto.Comment);
    }

    private void RemoveContent(
        Sale sale,
        SaleContent content)
    {
        sale.RemoveContent(content);
        unitOfWork.Remove(content);
    }

    private async Task UpdateReservationsCounts(
        Sale sale,
        IReadOnlyDictionary<int, int> oldCounts,
        CancellationToken cancellationToken)
    {
        var deltas = sale.Contents
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(z => z.Count) - oldCounts.GetValueOrDefault(x.Key));

        await saleService.UpdateReservationsCounts(sale.BuyerId, deltas, cancellationToken);
    }
}
