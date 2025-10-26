using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Core.Interfaces.Services;
using Contracts.Models.Sale;
using Contracts.Sale;
using Core.Attributes;
using Core.Interfaces.MessageBroker;
using Core.Interfaces.Services;

namespace Analytics.Application.EventHandlers;

public class SaleEditedEventHandler(IUnitOfWork unitOfWork, ISellInfoService sellInfoService) : IEventHandler<SaleEditedEvent>
{
    public async Task HandleAsync(IEventContext<SaleEditedEvent> context)
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