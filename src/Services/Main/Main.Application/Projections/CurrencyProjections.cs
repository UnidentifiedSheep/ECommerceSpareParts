using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Main.Application.Dtos.Currencies;
using Main.Entities.Currency;

namespace Main.Application.Projections;

public sealed class CurrencyDtoProjectionProvider
    : ISingletonProjectionProvider<Currency, CurrencyDto>
{
    public Expression<Func<Currency, CurrencyDto>> Projection { get; } =
        x => new CurrencyDto
        {
            Id = x.Id,
            Name = x.Name,
            ShortName = x.ShortName,
            CurrencySign = x.CurrencySign,
            Code = x.Code
        };
}
