using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models;
using Main.Application.Dtos.Amw.Sales;
using Main.Entities.Sale;
using Main.Entities.Storage;

namespace Main.Application.Handlers.Sales.CreateSale;

[AutoSave]
[Transactional(IsolationLevel.Serializable, 20, 2)]
public record CreateSaleCommand(
    IEnumerable<NewSaleContentDto> SellContent,
    IEnumerable<PrevAndNewValue<StorageContent>> StorageContentValues,
    int CurrencyId,
    Guid BuyerId,
    Guid TransactionId,
    string Storage,
    DateTime SaleDateTime,
    string? Comment) : ICommand<CreateSaleResult>;

public record CreateSaleResult(Sale Sale);

public class CreateSaleHandler(ISaleService saleService, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSaleCommand, CreateSaleResult>
{
    public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var transactionId = request.TransactionId;
        var buyerId = request.BuyerId;
        var currencyId = request.CurrencyId;
        var storageName = request.Storage;

        Sale sale = Sale.Create(buyerId, transactionId, currencyId, storageName, request.SaleDateTime);
        sale.SetComment(request.Comment);

        var saleContentList = request.SellContent.ToList();
        var detailGroups = saleService.GetDetailsGroup(request.StorageContentValues);

        foreach (var newContent in saleContentList)
        {
            var saleContent = SaleContent.Create(
                newContent.Count,
                newContent.Price,
                newContent.PriceWithDiscount,
                newContent.Count,
                detailGroups[newContent.ProductId]);
            
            sale.AddContent(saleContent);
        }
        
        await unitOfWork.AddAsync(sale, cancellationToken);
        return new CreateSaleResult(sale);
    }
}