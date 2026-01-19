namespace Main.Application.Extensions;

public static class Price
{
    extension(double value)
    {
        public double RoundToNearestUp()
        {
            return (int)(value * 100 + 0.5) / 100.0;
        }

        public double GetMarkUppedPrice(double markup)
        {
            return value * (1 + markup / 100);
        }

        public double GetDiscountedPrice(double discount)
        {
            return value * (1 - discount / 100);
        }
    }
}