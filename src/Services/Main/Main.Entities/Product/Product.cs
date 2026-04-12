using BulkValidation.Core.Attributes;
using Domain;
using Main.Entities.Product.ValueObjects;

namespace Main.Entities.Product;

public class Product : AuditableEntity<Product, int>
{
    [Validate]
    public int Id { get; private set; }
    public int? PairId { get; private set; }
    public Sku Sku { get; set; } = null!;
    public Name Name { get; set; } = null!;
    public Stock Stock { get; set; } = null!;
    public string? Description { get; private set; }
    public int? PackingUnit { get; private set; }
    public int ProducerId { get; private set; }
    public Indicator? Indicator { get; private set; }
    public int? CategoryId { get; private set; }
    public long Popularity { get; private set; }

    private List<ProductCharacteristic> _characteristics = [];
    public IReadOnlyCollection<ProductCharacteristic> Characteristics => _characteristics;

    private List<ProductEan> _eans = [];
    public IReadOnlyCollection<ProductEan> Eans => _eans;

    private List<ProductImage> _images = [];
    public IReadOnlyCollection<ProductImage> Images => _images;

    private List<ProductContent> _contents = [];
    public IReadOnlyCollection<ProductContent> Contents => _contents;
    public ProductSize? ProductSize { get; private set; }

    public ProductWeight? ProductWeight { get; private set; }

    public Product? Pair { get; private set; }

    public Category? Category { get; private set; }

    public Producer.Producer Producer { get; private set; } = null!;


    private Product(
        Sku sku, 
        Name name, 
        int producerId, 
        string? description)
    {
        Sku = sku;
        Name = name;
        ProducerId = producerId;
        Description = description;
        Stock = 0;
    }
    
    private Product() {}

    public static Product Create(
        Sku sku, 
        Name name, 
        int producerId, 
        string? description)
    {
        return new Product(sku, name, producerId, description);
    }

    public void SetDescription(string? description)
    {
        description = description?.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description;
    }

    public void SetSku(Sku sku)
    {
        Sku = sku;
    }

    public void SetName(Name name)
    {
        Name = name;
    }

    public void SetProducerId(int producerId)
    {
        ProducerId = producerId;
    }

    public void UpdateStock(Stock stock)
    {
        Stock = stock;
    }

    public void IncreaseStock(int value)
    {
        Stock = Stock.Value + value;
    }

    public void SetPackingUnit(int? packingUnit)
    {
        if (packingUnit.HasValue)
            ArgumentOutOfRangeException.ThrowIfNegative(packingUnit.Value);
        PackingUnit = packingUnit;
    }

    public void SetIndicator(Indicator indicator)
    {
        Indicator = indicator;
    }

    public void SetCategory(int? categoryId)
    {
        CategoryId = categoryId;
    }

    public void SetPopularity(long popularity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(popularity);
        Popularity = popularity;
    }

    public void SetPair(int? pairId)
    {
        PairId = pairId;
    }
    
    public override int GetId() => Id;
}