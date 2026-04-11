using BulkValidation.Core.Attributes;
using Domain;

namespace Main.Entities.Product;

public class Product : AuditableEntity<Product, int>
{
    [Validate]
    public int Id { get; set; }
    
    public int? PairId { get; set; }

    public string Sku { get; set; } = null!;

    public string NormalizedSku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? PackingUnit { get; set; }

    public int ProducerId { get; set; }

    public int Stock { get; set; }

    public string? Indicator { get; set; }

    public int? CategoryId { get; set; }
    public long Popularity { get; set; }

    public virtual ICollection<ProductCharacteristic> ProductCharacteristics { get; set; } =
        new List<ProductCharacteristic>();

    public ICollection<ProductCoefficient> ProductCoefficients { get; set; } = new List<ProductCoefficient>();

    public ICollection<ProductEan> ProductEans { get; set; } = new List<ProductEan>();

    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public ICollection<ProductContent> ProductContents { get; set; } = new List<ProductContent>();
    public virtual ProductSize? ProductSize { get; set; }

    public virtual ProductWeight? ProductWeight { get; set; }

    public Product? Pair { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Producer.Producer Producer { get; set; } = null!;
    public override int GetId() => Id;
}