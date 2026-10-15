using Enums.Units;

namespace Contracts.Products;

public record ProductSnapshot
{
	public required int Id { get; init; }
	public string? Sku { get; init; }
	public string? Name { get; init; }
	public int ProducerId { get; init; }
	public int? ProductGroupId { get; init; }
	public int Stock { get; init; }
	public string? Indicator { get; init; }
	public bool IsDeleted { get; init; }
}

public record ProductCharacteristicSnapshot(int CharacteristicId, string Value);
public record ProductCharacteristicsSnapshot(int ProductId, IReadOnlyList<ProductCharacteristicSnapshot> Characteristics);
public record ProductDimensionsSnapshot(int ProductId, ProductDimensionsData? Dimensions);
public record ProductWeightSnapshot(int ProductId, ProductWeightData? Weight);
public record ProductDimensionsData(decimal Length, decimal Width, decimal Height, DimensionUnit Unit);
public record ProductWeightData(decimal Value, WeightUnit Unit);
