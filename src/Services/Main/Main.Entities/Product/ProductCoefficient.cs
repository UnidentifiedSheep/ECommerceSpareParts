using Domain;

namespace Main.Entities;

public class ProductCoefficient : AuditableEntity<ProductCoefficient, (int, string)>
{
    public int ProductId { get; set; }

    public string CoefficientName { get; set; } = null!;

    public DateTime ValidTill { get; set; }
    
    public virtual Coefficient Coefficient { get; set; } = null!;
    
    public override (int, string) GetId() => (ProductId, CoefficientName);
}