namespace Pricing.Abstractions.Models.Pricing;

public record ArticlePrice
{
    public ArticlePrice(decimal Price, decimal DeliveryPrice)
    {
        if (Price <= 0) 
            throw new ArgumentOutOfRangeException(nameof(Price), "Цена не может быть меньше или равна нулю.");
        if (DeliveryPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(DeliveryPrice), "Цена доставки не может быть меньше нуля.");
        this.Price = Price;
        this.DeliveryPrice = DeliveryPrice;
    }

    public decimal Price { get; init; }
    public decimal DeliveryPrice { get; init; }

    public void Deconstruct(out decimal price, out decimal deliveryPrice)
    {
        price = Price;
        deliveryPrice = DeliveryPrice;
    }
}