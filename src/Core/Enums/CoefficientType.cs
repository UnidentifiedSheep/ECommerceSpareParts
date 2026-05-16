namespace Enums;

public enum CoefficientType
{
    /// <summary>
    ///     Простой множитель: цена умножается на коэф.
    /// </summary>
    Multiplicative = 0,

    /// <summary>
    ///     Процент от базовой цены: Value в процентах.
    /// </summary>
    PercentOfBase = 1,

    /// <summary>
    ///     Фиксированная надбавка/скидка: коэф прибавляется к цене.
    /// </summary>
    Additive = 2
}