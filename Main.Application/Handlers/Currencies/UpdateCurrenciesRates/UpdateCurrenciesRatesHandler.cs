using Core.Attributes;
using Core.Contracts;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Integrations;
using Core.Interfaces.Services;
using Main.Application.Interfaces;
using MediatR;

namespace Main.Application.Handlers.Currencies.UpdateCurrenciesRates;

[Transactional]
public record UpdateCurrenciesRatesCommand : ICommand;

public class UpdateCurrenciesRatesHandler(
    IExchangeRates exchange,
    IMessageBroker messageBroker,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateCurrenciesRatesCommand>
{
    public async Task<Unit> Handle(UpdateCurrenciesRatesCommand request, CancellationToken cancellationToken)
    {
        var currencies = (await currencyRepository.GetCurrencies([Global.UsdId], true, cancellationToken))
            .ToList();
        var currencyCodes = currencies.Select(x => x.Code);
        var newRates = await exchange.GetRates(currencyCodes, "USD", cancellationToken);

        var historyToAdd = new List<CurrencyHistory>();
        foreach (var rate in newRates.Rates)
        {
            var currency = currencies.FirstOrDefault(x => x.Code == rate.Key);
            if (currency == null) continue;

            var prevValue = currency.CurrencyToUsd?.ToUsd ?? 0;
            if (currency.CurrencyToUsd == null)
                currency.CurrencyToUsd = new CurrencyToUsd
                {
                    CurrencyId = currency.Id,
                    ToUsd = rate.Value
                };
            else
                currency.CurrencyToUsd.ToUsd = rate.Value;

            var currencyHistory = new CurrencyHistory
            {
                CurrencyId = currency.Id,
                PrevValue = prevValue,
                NewValue = rate.Value
            };
            if (prevValue != rate.Value)
                historyToAdd.Add(currencyHistory);
        }

        await unitOfWork.AddRangeAsync(historyToAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await messageBroker.Publish(new CurrencyRateChangedEvent(), cancellationToken);
        return Unit.Value;
    }
}