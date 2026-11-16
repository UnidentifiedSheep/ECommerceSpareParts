using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Modules.Currency.Application.Dtos;

namespace Main.Modules.Currency.Application;

[Lifetime(Lifetime.Singleton)]
public sealed class CurrencyDtoProjectionProvider : ProjectionProviderBase<Entities.Currency.Currency, CurrencyDto>
{
	public override Expression<Func<Entities.Currency.Currency, CurrencyDto>> Projection { get; } = x => new CurrencyDto
	{
		Id = x.Id,
		Name = x.Name,
		ShortName = x.ShortName,
		CurrencySign = x.CurrencySign,
		Code = x.Code
	};
}
