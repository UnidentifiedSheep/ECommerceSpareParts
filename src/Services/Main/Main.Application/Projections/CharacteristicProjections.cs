using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Entities.Product;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class CharacteristicDtoProjectionProvider : ProjectionProviderBase<Characteristic, CharacteristicDto>
{
	public override Expression<Func<Characteristic, CharacteristicDto>> Projection { get; } = characteristic =>
		new CharacteristicDto
		{
			Id = characteristic.Id,
			Name = characteristic.Name,
			NormalizedName = characteristic.NormalizedName
		};
}
