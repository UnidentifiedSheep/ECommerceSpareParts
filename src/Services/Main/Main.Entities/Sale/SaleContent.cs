namespace Main.Entities;

public class SaleContent
{
    public int Id { get; set; }

    public string SaleId { get; set; } = null!;

    public int ProductId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public decimal TotalSum { get; set; }

    public string? Comment { get; set; }

    public decimal Discount { get; set; }

    public virtual Product.Product Product { get; set; } = null!;

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();
}