using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Product;

public class ProductCoefficient : AuditableEntity<ProductCoefficient, (int, string)>,
    ILinqEntity<ProductCoefficient, (int, string)>
{
    public int ProductId { get; set; }

    public string CoefficientName { get; set; } = null!;

    public DateTime ValidTill { get; set; }

    public virtual Coefficient Coefficient { get; set; } = null!;

    public static Expression<Func<ProductCoefficient, bool>> GetEqualityExpression((int, string) key)
    {
        return x => x.ProductId == key.Item1 && x.CoefficientName == key.Item2;
    }

    public override (int, string) GetId()
    {
        return (ProductId, CoefficientName);
    }
}