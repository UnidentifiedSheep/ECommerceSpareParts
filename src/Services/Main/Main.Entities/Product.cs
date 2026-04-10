using BulkValidation.Core.Attributes;

namespace Main.Entities;

public class Product
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

    public virtual ICollection<ProductCharacteristic> ArticleCharacteristics { get; set; } =
        new List<ProductCharacteristic>();

    public virtual ICollection<ArticleCoefficient> ArticleCoefficients { get; set; } = new List<ArticleCoefficient>();

    public virtual ICollection<ArticleEan> ArticleEans { get; set; } = new List<ArticleEan>();

    public virtual ICollection<ArticleImage> ArticleImages { get; set; } = new List<ArticleImage>();

    public virtual ArticleSize? ArticleSize { get; set; }

    public virtual ArticleWeight? ArticleWeight { get; set; }

    public virtual ICollection<ArticlesContent> ArticlesContentMainArticles { get; set; } = new List<ArticlesContent>();

    public Product? Pair { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Producer Producer { get; set; } = null!;
}