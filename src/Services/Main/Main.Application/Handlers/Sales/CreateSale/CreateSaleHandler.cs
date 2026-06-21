using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Models.Options;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Products;
using Contracts.Sale;
using Contracts.StorageContent;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Interfaces.Services;
using Main.Application.Projections;
using Main.Entities.Sale;
using Main.Entities.Setting;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Sales.CreateSale;

[Transactional(IsolationLevel.Serializable, 30, 2)]
[AutoSave]
public record CreateSaleCommand(
    Guid BuyerId,
    int CurrencyId,
    string StorageName,
    DateTime SaleDateTime,
    IEnumerable<NewSaleContentDto> Contents,
    string? Comment,
    decimal? PayedSum,
    string? ConfirmationCode) : ICommand<CreateSaleResult>;

public record CreateSaleResult(SaleDto Sale);

public class CreateSaleHandler(
    ISender sender,
    IOptions<SystemOptions> systemOptions,
    IIntegrationEventScope integrationEventScope,
    IReadRepository<Sale, Guid> readRepository,
    IUnitOfWork unitOfWork,
    ISaleService saleService) : ICommandHandler<CreateSaleCommand, CreateSaleResult>
{
    public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var contents = request.Contents.ToList();
        
        await saleService.CheckReservations(
            contents, 
            request.BuyerId, 
            request.StorageName,
            false,
            request.ConfirmationCode, 
            cancellationToken);
        
        var systemId = systemOptions.Value.SystemId;

        var totalSum = contents.Sum(x => x.PriceWithDiscount * x.Count);

        var saleTransaction = (await sender.Send(
            new CreateTransactionCommand(
                systemId,
                request.BuyerId,
                totalSum,
                request.CurrencyId,
                request.SaleDateTime,
                TransactionSourceType.Sale,
                TransactionCreationMode.System),
            cancellationToken)).Transaction;

        if (request.PayedSum > 0)
            await sender.Send(
                new CreateTransactionCommand(
                    request.BuyerId,
                    systemId,
                    request.PayedSum.Value,
                    request.CurrencyId,
                    request.SaleDateTime,
                    TransactionSourceType.Manual,
                    TransactionCreationMode.System),
                cancellationToken);
        
        var sale = Sale.Create(
            request.BuyerId,
            saleTransaction.Id,
            request.CurrencyId,
            request.StorageName,
            request.SaleDateTime);

        sale.SetComment(request.Comment);
        
        var distributed = await saleService.TakeFromStorageAndDistributeDetails(
            request.StorageName,
            contents,
            StorageMovementType.Sale,
            false,
            cancellationToken);

        foreach (var content in distributed)
        {
            sale.AddContent(content);
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = content.ProductId,
            });
            
            integrationEventScope.Add(new StorageContentUpdatedEvent
            {
                ProductId = content.ProductId,
            });
        }
        
        sale.Complete();

        await saleService.SubtractCountFromReservations(sale, request.BuyerId, cancellationToken);

        await unitOfWork.AddAsync(sale, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        integrationEventScope.Add(new SaleUpdatedEvent
        {
            SaleId = sale.GetId()
        });

        return await ReturnAsync(sale.GetId(), cancellationToken);
    }

    private async Task<CreateSaleResult> ReturnAsync(
        Guid id,
        CancellationToken token)
    {
        var fromDb = await readRepository.Query
            .AsExpandable()
            .Select(SaleProjections.ToSaleDto)
            .FirstAsync(x => x.Id == id, token);

        return new CreateSaleResult(fromDb);
    }
}
