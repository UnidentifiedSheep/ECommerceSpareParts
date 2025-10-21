using Analytics.Core.Entities;
using Analytics.Core.Static;
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
                    Markup = Calculate.Markup(avrgBuyPrice, content.Price),
                    SellDate = sell.SaleDatetime
                };
                sellInfos.Add(sellInfo);
            }
            await unitOfWork.AddRangeAsync(sellInfos);
            await unitOfWork.SaveChangesAsync();
        });
    }
}