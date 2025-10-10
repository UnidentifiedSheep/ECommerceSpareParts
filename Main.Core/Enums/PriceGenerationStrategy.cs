namespace Main.Core.Enums;

public enum PriceGenerationStrategy
{
    /// <summary>Самая высокая цена по позиции</summary>
    TakeHighestPrice,

    /// <summary>Средняя цена самых дорогих 5 позиций</summary>
    TakeAverageOfTop5Prices,

    /// <summary>Средняя цена по всем позициям</summary>
    TakeAveragePrice
}