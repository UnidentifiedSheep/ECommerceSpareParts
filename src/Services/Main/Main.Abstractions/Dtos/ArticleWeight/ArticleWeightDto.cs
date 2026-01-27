using Main.Enums;

namespace Main.Abstractions.Dtos.ArticleWeight;

public class ArticleWeightDto
{
    public int ArticleId { get; set; }
    public decimal Weight { get; set; }
    public WeightUnit Unit { get; set; }
}