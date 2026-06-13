namespace Search.Entities;

public class Product
{
    public int Id { get; init; }

    public required string Sku { get; init; }

    public required string NormalizedSku { get; init; }

    public required string Name { get; init; }

    public required int ProducerId { get; init; }
    public required int Stock { get; init; }

    public ProductDimensions? Dimensions { get; init; }

    public ProductWeight? Weight { get; init; }
}
