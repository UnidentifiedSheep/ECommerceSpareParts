namespace Main.Application.Extensions;

public static class Price
{
    extension(decimal value)
    {
        public decimal GetDiscountedPrice(decimal discountFraction)
        {
            return value * (1 - discountFraction);
        }

        public decimal GetMarkUppedPrice(decimal markupFraction)
        {
            return value * (1 + markupFraction);
        }
    }
    
    public static decimal GetDiscountFromPrices(decimal withDiscount, decimal withOutDiscount)
    {
        return (withOutDiscount - withDiscount) / withOutDiscount * 100;
    }
}