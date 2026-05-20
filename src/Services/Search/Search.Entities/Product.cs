namespace Search.Entities;

public class Product
{
    public int Id { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    
    public required int ProducerId { get; init; }
}