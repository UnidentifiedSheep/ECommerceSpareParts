using Abstractions.Interfaces.Services;
using Analytics.Core.Interfaces.Services;
using Attributes;
using Contracts.Models.Sale;
using Contracts.Sale;
using MassTransit;

namespace Analytics.Application.Consumers;

public class SaleEditedConsumer(IUnitOfWork unitOfWork, ISellInfoService sellInfoService) : IConsumer<SaleEditedEvent>
{
    public async Task Consume(ConsumeContext<SaleEditedEvent> context)
    {
        var deleted = context.Message.DeletedSaleContents.ToList();
        var sale = context.Message.Sale;
        Validate(sale, deleted);
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
        {
            await sellInfoService.RemoveSellInfos(deleted);
            await sellInfoService.EditSellInfos(sale);
            await unitOfWork.SaveChangesAsync();
        });
    }
    
    private void Validate(Sale sale, IEnumerable<int> deletedSaleContents)
    {
        var existing = sale.SaleContents.Select(x => x.Id).ToHashSet();
        existing.IntersectWith(deletedSaleContents);
        if (existing.Count > 0)
            throw new ArgumentException("Удаленная позиция не может содержаться в Закупке.");
    }

}