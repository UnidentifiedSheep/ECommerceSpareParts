namespace Main.Entities;

public partial class PurchaseContentLogistic
{
    public int PurchaseContentId { get; set; }

    public int WeightG { get; set; }

    public int AreaCm3 { get; set; }

    public decimal Price { get; set; }

    public virtual PurchaseContent PurchaseContent { get; set; } = null!;
}
