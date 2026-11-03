using Search.Enums;
namespace Search.Entities;

public class Product
{
	public int Id { get; init; }

	public required string Sku { get; init; }

	public required string NormalizedSku { get; init; }

	public required string Name { get; init; }

	public required int ProducerId { get; init; }

	public int? ProductGroupId { get; init; }

	public required string? Indicator { get; init; }

	public ProductDimensions? Dimensions { get; init; }
	public ProductWeight? Weight { get; init; }
	public IReadOnlyList<ProductCharacteristic> Characteristics { get; init; } = [];

	public IReadOnlyDictionary<string, int> Stocks { get; init; } = new Dictionary<string, int>();
	public long? Stock { get; init; }
	public long? StockSnapshotVersion { get; init; }
	public IReadOnlyDictionary<string, long> StockVersions { get; init; } = new Dictionary<string, long>();

	public ProductReadinessStatus ReadinessStatus { get; init; } = ProductReadinessStatus.Partial;
	public DateTime? ProductUpdatedAt { get; init; }
	public DateTime? CharacteristicsUpdatedAt { get; init; }
	public DateTime? DimensionsUpdatedAt { get; init; }
	public DateTime? WeightUpdatedAt { get; init; }
}
