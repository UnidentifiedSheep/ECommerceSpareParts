namespace Main.Application.Dtos.Amw.Sales;

public class NewSaleContentDto
{
    public int ProductId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public decimal PriceWithDiscount { get; set; }
    public string? Comment { get; set; }
}