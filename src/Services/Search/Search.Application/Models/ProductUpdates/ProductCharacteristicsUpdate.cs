using Contracts.Products;
using System.Globalization;
using System.Linq.Expressions;
using Search.Entities;

namespace Search.Application.Models.ProductUpdates;

public sealed record ProductCharacteristicsUpdate : ProductFieldUpdate
{
	public IReadOnlyList<ProductCharacteristic> Characteristics { get; init; } = [];

	public static ProductCharacteristicsUpdate From(ProductCharacteristicsSnapshot product, DateTime updatedAt) => new()
	{
		ProductId = product.ProductId,
		UpdatedAt = updatedAt,
		Characteristics = product.Characteristics.Select(characteristic => new ProductCharacteristic(
			characteristic.CharacteristicId, characteristic.Value, ParseNumber(characteristic.Value))).ToArray()
	};

	private static decimal? ParseNumber(string value) =>
		decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : null;

	public override Expression<Func<Product, DateTime?>> GetUpdatedAtField() => product => product.CharacteristicsUpdatedAt;
}
