using System.Linq.Expressions;
using Main.Abstractions.Dtos.Currencies;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.Projections;

public static class CurrencyProjections
{
    public static readonly Expression<Func<Currency, CurrencyDto>> ToDto =
        x => new CurrencyDto
        {
            Id = x.Id,
            Name = x.Name,
            ShortName = x.ShortName,
            CurrencySign = x.CurrencySign,
            Code = x.Code,
            ToUsdRate = x.CurrencyToUsd == null ? null : x.CurrencyToUsd.ToUsd
        };
}