using Core.Attributes;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Currencies;
using Main.Application.Interfaces;
using MediatR;

namespace Main.Application.Handlers.BuySellPrices.EditBuySellPrices;

[Transactional]
public record EditBuySellPricesCommand(IEnumerable<SaleContent> SaleContents, int CurrencyId) : ICommand;

public class EditBuySellPricesHandler(
    IBuySellPriceRepository bsRepository,
    IUnitOfWork unitOfWork,
    ICurrencyConverter currencyConverter) : ICommandHandler<EditBuySellPricesCommand>
{
    public async Task<Unit> Handle(EditBuySellPricesCommand request, CancellationToken cancellationToken)
    {
        var currencyId = request.CurrencyId;

        ValidateData(currencyId);

        var saleContentsDict = request.SaleContents.ToDictionary(x => x.Id);
        var buySellPrices = await bsRepository.GetBsPriceByContentIdsForUpdate(saleContentsDict.Keys,
            true, cancellationToken);

        foreach (var bsPrice in buySellPrices)
        {
            var sContent = saleContentsDict[bsPrice.SaleContentId];
            if (bsPrice.CurrencyId == currencyId && bsPrice.SellPrice == sContent.Price) continue;
            bsPrice.SellPrice = Math.Round(currencyConverter.ConvertTo(sContent.Price, bsPrice.CurrencyId, currencyId),
                2);
            bsPrice.CurrencyId = currencyId;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void ValidateData(int currencyId)
    {
        if (!currencyConverter.IsSupportedCurrency(currencyId))
            throw new CurrencyNotFoundException(currencyId);
    }
}