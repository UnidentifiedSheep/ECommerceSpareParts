namespace Main.Abstractions.Dtos.Amw.Purchase;

public class PurchaseContentLogisticDto
{
    public int PurchaseContentId { get; set; }

    public decimal WeightKg { get; set; }

    public decimal AreaM3 { get; set; }

    public decimal Price { get; set; }
}