using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Application.Dtos.Currencies;
using Main.Entities.Currency;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class CurrencyDtoProjectionProvider : ProjectionProviderBase<Currency, CurrencyDto>
{
	public override Expression<Func<Currency, CurrencyDto>> Projection { get; } = x => new CurrencyDto
	{
		Id = x.Id,
		Name = x.Name,
		ShortName = x.ShortName,
		CurrencySign = x.CurrencySign,
		Code = x.Code
	};
}
