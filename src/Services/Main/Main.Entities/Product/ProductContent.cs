namespace Main.Entities.Product;

public class ProductContent
{
    public int ParentProductId { get; set; }

    public int ChildProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Product ParentProduct { get; set; } = null!;
    public virtual Product ChildProduct { get; set; } = null!;
}