using System.Linq.Expressions;
using Domain;

namespace Main.Entities.Product;

public class ProductCoefficient : AuditableEntity<ProductCoefficient, (int, string)>
{
    public int ProductId { get; set; }

    public string CoefficientName { get; set; } = null!;

    public DateTime ValidTill { get; set; }

    public virtual Coefficient Coefficient { get; set; } = null!;

    public override (int, string) GetId()
    {
        return (ProductId, CoefficientName);
    }

    public override Expression<Func<ProductCoefficient, bool>> GetEqualityExpression((int, string) key)
        => x => x.ProductId == key.Item1 && x.CoefficientName == key.Item2;
}