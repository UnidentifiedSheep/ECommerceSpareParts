using BulkValidation.Core.Attributes;

namespace Main.Entities;

public class PurchaseContent
{
    [Validate]
    public int Id { get; set; }

    public string PurchaseId { get; set; } = null!;

    public int ProductId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public decimal TotalSum { get; set; }

    public string? Comment { get; set; }

    public int? StorageContentId { get; set; }

    public virtual Product.Product Product { get; set; } = null!;

    public virtual PurchaseContentLogistic? PurchaseContentLogistic { get; set; }
}