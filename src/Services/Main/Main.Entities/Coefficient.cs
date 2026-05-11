using Enums;
using Main.Entities.Product;

namespace Main.Entities;

public class Coefficient
{
    public string Name { get; set; } = null!;

    public decimal Value { get; set; }

    public CoefficientType Type { get; set; }

    public virtual ICollection<ProductCoefficient> ProductCoefficients { get; set; } = new List<ProductCoefficient>();
}