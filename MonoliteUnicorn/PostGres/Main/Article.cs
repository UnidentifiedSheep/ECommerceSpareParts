using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class Article
{
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

    public virtual ICollection<ArticleImage> ArticleImages { get; set; } = new List<ArticleImage>();

    public virtual ICollection<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; } = new List<ArticleSupplierBuyInfo>();

    public virtual ICollection<ArticlesContent> ArticlesContentInsideArticles { get; set; } = new List<ArticlesContent>();

    public virtual ICollection<ArticlesContent> ArticlesContentMainArticles { get; set; } = new List<ArticlesContent>();

    public virtual Category? Category { get; set; }

    public virtual Producer Producer { get; set; } = null!;

    public virtual ICollection<PurchaseContent> PurchaseContents { get; set; } = new List<PurchaseContent>();

    public virtual ICollection<SaleContent> SaleContents { get; set; } = new List<SaleContent>();

    public virtual ICollection<StorageContentReservation> StorageContentReservations { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageContent> StorageContents { get; set; } = new List<StorageContent>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();
}
