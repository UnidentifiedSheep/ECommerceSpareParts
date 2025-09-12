using Application.Interfaces;
using Core.Attributes;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Services;
using MediatR;

namespace Application.Handlers.BuySellPrices.AddBuySellPrices;

[Transactional]
public record AddBuySellPricesCommand(
    IEnumerable<StorageContent> StorageContents,
    IEnumerable<SaleContent> SaleContents,
    int CurrencyId) : ICommand;

public class AddBuySellPricesHandler(ICurrencyConverter currencyConverter, IUnitOfWork unitOfWork)
    : ICommandHandler<AddBuySellPricesCommand>
{
    public async Task<Unit> Handle(AddBuySellPricesCommand request, CancellationToken cancellationToken)
    {
        var articleBuyPrices = request.StorageContents
            .GroupBy(x => x.ArticleId, x => x.BuyPriceInUsd)
            .ToDictionary(x => x.Key, x => x.Average());

        var bsList = new List<BuySellPrice>();
        foreach (var content in request.SaleContents)
        {
            var avrgBuyPrice = articleBuyPrices[content.ArticleId];
            var buySellPrices = new BuySellPrice
            {
                BuyPrice = Math.Round(currencyConverter.ConvertFromUsd(avrgBuyPrice, request.CurrencyId), 2),
                SellPrice = content.Price,
                CurrencyId = request.CurrencyId,
                SaleContentId = content.Id
            };
            bsList.Add(buySellPrices);
        }

        await unitOfWork.AddRangeAsync(bsList, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}