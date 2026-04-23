using System.Linq.Expressions;
using Main.Application.Dtos.Currencies;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Projections;

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