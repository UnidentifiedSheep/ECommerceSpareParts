using BulkValidation.Core.Attributes;

namespace Main.Entities;

public partial class Article
{
    [Validate]
    public int Id { get; set; }

    public string ArticleNumber { get; set; } = null!;

    public string NormalizedArticleNumber { get; set; } = null!;

    public string ArticleName { get; set; } = null!;

    public bool IsValid { get; set; }

    public string? Description { get; set; }

    public int? PackingUnit { get; set; }

    public int ProducerId { get; set; }

    public bool IsOe { get; set; }

    public int TotalCount { get; set; }

    public string? Indicator { get; set; }

    public int? CategoryId { get; set; }

    public virtual ICollection<ArticleCharacteristic> ArticleCharacteristics { get; set; } = new List<ArticleCharacteristic>();

    public virtual ICollection<ArticleCoefficient> ArticleCoefficients { get; set; } = new List<ArticleCoefficient>();

    public virtual ICollection<ArticleEan> ArticleEans { get; set; } = new List<ArticleEan>();

    public virtual ICollection<ArticleImage> ArticleImages { get; set; } = new List<ArticleImage>();

    public virtual ArticleSize? ArticleSize { get; set; }

    public virtual ICollection<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; } = new List<ArticleSupplierBuyInfo>();

    public virtual ArticleWeight? ArticleWeight { get; set; }

    public virtual ICollection<ArticlesContent> ArticlesContentInsideArticles { get; set; } = new List<ArticlesContent>();

    public virtual ICollection<ArticlesContent> ArticlesContentMainArticles { get; set; } = new List<ArticlesContent>();

    public virtual ArticlesPair? ArticlesPairArticleLeftNavigation { get; set; }

    public virtual ICollection<ArticlesPair> ArticlesPairArticleRightNavigations { get; set; } = new List<ArticlesPair>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Producer Producer { get; set; } = null!;

    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();

    public virtual ICollection<StorageContentReservation> StorageContentReservations { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageContent> StorageContents { get; set; } = new List<StorageContent>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();

    public virtual ICollection<Article> ArticleCrosses { get; set; } = new List<Article>();

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
