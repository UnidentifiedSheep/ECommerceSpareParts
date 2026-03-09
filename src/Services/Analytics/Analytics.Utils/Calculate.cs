namespace Analytics.Utils;

public static class Calculate
{
    /// <summary>
    /// Calculates coefficient of markup.
    /// </summary>
    /// <param name="buyPrice">Buy price of item</param>
    /// <param name="sellPrice">Sell price of item</param>
    /// <returns>Markup</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws when buy price or sell price is negative or zero</exception>
    public static decimal Markup(decimal buyPrice, decimal sellPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(buyPrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sellPrice);
        
        decimal markup = (sellPrice - buyPrice) / buyPrice;
        return Math.Round(markup, 2);
    }
}