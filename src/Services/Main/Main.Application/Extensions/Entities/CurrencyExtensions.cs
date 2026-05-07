using Contracts.Models.Currency;

namespace Application.Common.Extensions.Entities;

public static class CurrencyExtensions
{
    public static Currency ToContract(this Main.Entities.Currency.Currency model)
        => new()
        {
            Id = model.Id,
            Name = model.Name,
            CurrencySign = model.CurrencySign,
            Code = model.Code,
            ShortName = model.ShortName,
        };
}