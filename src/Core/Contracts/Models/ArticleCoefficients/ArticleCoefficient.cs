using Contracts.Models.Coefficients;

namespace Contracts.Models.ArticleCoefficients;

public class ArticleCoefficient
{
    public int ArticleId { get; set; }
    public DateTime ValidTill { get; set; }
    public DateTime CreatedAt { get; set; }
    public Coefficient Coefficient { get; set; } = null!;
}