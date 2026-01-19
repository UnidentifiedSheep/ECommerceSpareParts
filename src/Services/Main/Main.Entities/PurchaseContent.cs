namespace Main.Entities;

public partial class PurchaseContent
{
    public int Id { get; set; }

    public string PurchaseId { get; set; } = null!;

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public decimal TotalSum { get; set; }

    public string? Comment { get; set; }

    public int? StorageContentId { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Purchase Purchase { get; set; } = null!;

    public virtual PurchaseContentLogistic? PurchaseContentLogistic { get; set; }

    public virtual StorageContent? StorageContent { get; set; }
}
