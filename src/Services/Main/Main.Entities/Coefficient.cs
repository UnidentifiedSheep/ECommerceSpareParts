using Enums;
using Main.Entities.Product;

namespace Main.Entities;

public class Coefficient
{
    public string Name { get; set; } = null!;

    public int Order { get; set; }

    public decimal Value { get; set; }

    public CoefficientType Type { get; set; }

    public virtual ICollection<ProductCoefficient> ArticleCoefficients { get; set; } = new List<ProductCoefficient>();
}