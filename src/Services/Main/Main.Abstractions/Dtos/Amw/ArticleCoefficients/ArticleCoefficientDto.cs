using Main.Abstractions.Dtos.Amw.Coefficients;

namespace Main.Abstractions.Dtos.Amw.ArticleCoefficients;

public class ArticleCoefficientDto
{
    public int ArticleId { get; set; }
    public DateTime ValidTill { get; set; }
    public DateTime CreatedAt { get; set; }
    public CoefficientDto Coefficient { get; set; } = null!;
}