using Enums;
using Main.Enums;

namespace Main.Entities;

public partial class Coefficient
{
    public string Name { get; set; } = null!;

    public int Order { get; set; }

    public decimal Value { get; set; }

    public CoefficientType Type { get; set; }

    public virtual ICollection<ArticleCoefficient> ArticleCoefficients { get; set; } = new List<ArticleCoefficient>();
}
