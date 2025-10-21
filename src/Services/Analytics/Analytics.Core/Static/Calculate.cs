namespace Analytics.Core.Static;

public static class Calculate
{
    public static decimal Markup(decimal buyPrice, decimal sellPrice)
    {
        if (buyPrice == 0)
            throw new DivideByZeroException("Цена закупки не может равняться 0.");

        decimal markup = (sellPrice - buyPrice) / buyPrice * 100;
        return Math.Round(markup, 2);
    }
}