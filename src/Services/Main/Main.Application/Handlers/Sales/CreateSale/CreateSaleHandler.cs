using System.Data;
using System.Text;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Products;
using Contracts.Sale;
using Contracts.StorageContent;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;
using Main.Application.Handlers.ProductReservations.UpdateReservationsCounts;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Sale;
using Main.Entities.Setting;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utils;

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
    ISettingsService settingsService,
    IProductRepository productRepository,
    IIntegrationEventScope integrationEventScope,
    IReadRepository<Sale, Guid> readRepository,
    IUnitOfWork unitOfWork,
    ISaleService saleService) : ICommandHandler<CreateSaleCommand, CreateSaleResult>
{
    public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var contents = request.Contents.ToList();
        
        await CheckReservations(
            contents, 
            request.BuyerId, 
            request.StorageName,
            request.ConfirmationCode, 
            cancellationToken);
        
        var systemId = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data.SystemId;

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
        
        var takenFromStorage = (await sender.Send(
            new SubtractStorageContentsCommand(
                request.Contents.Select(x =>
                    new SubtractProductFromStorageItem(
                        x.ProductId,
                        request.StorageName,
                        x.Count)),
                StorageMovementType.Sale),
            cancellationToken));
        
        var distributed = saleService.DistributeDetails(
            takenFromStorage.Contents,
            contents);

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

        await SubtractCountFromReservations(sale, request.BuyerId, cancellationToken);

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
    
    private async Task SubtractCountFromReservations(
        Sale sale,
        Guid buyerId,
        CancellationToken cancellation = default)
    {
        var toSubtract = sale.Contents
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));
        var command = new UpdateReservationsCountsCommand(buyerId, toSubtract);
        await sender.Send(command, cancellation);
    }
    
    private async Task CheckReservations(
        List<NewSaleContentDto> saleContent,
        Guid buyerId,
        string storageName,
        string? confirmationCode,
        CancellationToken cancellationToken)
    {
        var (byReservation, byStock) = 
            await GetStockReservations(
                saleContent, 
                buyerId, 
                storageName, 
                cancellationToken);

        if (byStock.Count != 0)
            throw new NotEnoughCountOnStorageException(byStock.Keys);

        if (byReservation.Count != 0)
        {
            var criteria = Criteria<Product>.New()
                .Track(false)
                .Include(x => x.Producer)
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
        List<NewSaleContentDto> saleContent,
        Guid buyerId,
        string storageName,
        CancellationToken cancellationToken = default)
    {
        var neededProductCounts = saleContent
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key,
                x => x.Sum(z => z.Count));

        var result = await sender.Send(
            new GetProductsWithNotEnoughStockQuery(
                buyerId, 
                storageName,
                false,
                neededProductCounts), 
            cancellationToken);

        return (result.NotEnoughByReservation, result.NotEnoughByStock);
    }
}
