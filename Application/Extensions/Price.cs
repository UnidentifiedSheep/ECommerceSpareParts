namespace Application.Extensions;

public static class Price
{
    public static double RoundToNearestUp(this double value)
    {
        return (int)(value * 100 + 0.5) / 100.0;
    }

    public static double GetMarkUppedPrice(this double price, double markup)
    {
        return price * (1 + markup / 100);
    }

    public static double GetDiscountedPrice(this double price, double discount)
    {
        return price * (1 - discount / 100);
    }
}