using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Dtos.Product;

namespace Main.Modules.Product.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	CharacteristicDtoProjectionProvider : ProjectionProviderBase<Characteristic, CharacteristicDto>
{
	public override Expression<Func<Characteristic, CharacteristicDto>> Projection { get; } = characteristic
		=>
		new CharacteristicDto
		{
			Id = characteristic.Id,
			Name = characteristic.Name,
			NormalizedName = characteristic.NormalizedName
		};
}
