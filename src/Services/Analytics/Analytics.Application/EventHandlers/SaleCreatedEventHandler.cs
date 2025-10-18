using Analytics.Core.Entities;
using Contracts.Models.Sale;
using Contracts.Sale;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.MessageBroker;
using Core.Interfaces.Services;

namespace Analytics.Application.EventHandlers;

public class SaleCreatedEventHandler(ICurrencyConverter currencyConverter, IUnitOfWork unitOfWork) : IEventHandler<SaleCreatedEvent>
{
    public async Task HandleAsync(IEventContext<SaleCreatedEvent> context)
    {
        await unitOfWork.ExecuteWithTransaction(new TransactionalAttribute(), async () =>
        {
            var sell = context.Message.Sale;
            var sellInfos = new List<SellInfo>();
            foreach (SaleContent content in sell.SaleContents)
            {
                var avrgBuyPrice = content.Details
                    .Select(x => currencyConverter.ConvertTo(x.BuyPrice, x.CurrencyId, sell.CurrencyId))
                    .Average();
                SellInfo sellInfo = new SellInfo
                {
                    ArticleId = content.ArticleId,
                    BuyCurrencyId = sell.CurrencyId,
                    BuyPrices = avrgBuyPrice,
                    StorageName = sell.MainStorageName,
                    SellPrice = content.Price,
                    SellCurrencyId = sell.CurrencyId,
                    SellContentId = content.Id,
                    Markup = GetMarkup(avrgBuyPrice, content.Price)
                };
                sellInfos.Add(sellInfo);
            }
            await unitOfWork.AddRangeAsync(sellInfos);
            await unitOfWork.SaveChangesAsync();
        });
    }

    private decimal GetMarkup(decimal buyPrice, decimal sellPrice)
    {
        if (buyPrice == 0)
            throw new DivideByZeroException("Цена закупки не может равняться 0.");

        decimal markup = (sellPrice - buyPrice) / buyPrice * 100;
        return Math.Round(markup, 2);
    }
}