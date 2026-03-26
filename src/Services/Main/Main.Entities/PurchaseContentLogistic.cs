namespace Main.Entities;

public class PurchaseContentLogistic
{
    public int PurchaseContentId { get; set; }

    public decimal WeightKg { get; set; }

    public decimal AreaM3 { get; set; }

    public decimal Price { get; set; }

    public virtual PurchaseContent PurchaseContent { get; set; } = null!;
}