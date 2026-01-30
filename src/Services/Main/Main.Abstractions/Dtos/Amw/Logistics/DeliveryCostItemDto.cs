using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Logistics;

public class DeliveryCostItemDto
{
    public int ArticleId { get; set; }
    public decimal Cost { get; set; }
    public int Quantity { get; set; }
    public decimal AreaM3 { get; set; }
    public decimal AreaPerItem { get; set; }
    public decimal Weight { get; set; }
    public decimal WeightPerItem { get; set; }
    public WeightUnit WeightUnit { get; set; }
    public bool Skipped { get; set; }
    public IEnumerable<string>? Reasons { get; set; }

}