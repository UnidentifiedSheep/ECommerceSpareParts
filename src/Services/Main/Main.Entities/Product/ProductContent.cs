namespace Main.Entities.Product;

public class ProductContent
{
    public int ParentProductId { get; set; }

    public int ChildProductId { get; set; }

    public int Quantity { get; set; }
}