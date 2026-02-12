namespace Enums;

public enum CoefficientType
{
    /// <summary>
    /// Простой множитель: цена умножается на коэф.
    /// Не влияет на порядок
    /// </summary>
    Multiplicative = 0, 
    
    /// <summary>
    /// Процент от базовой цены: Value в процентах.
    /// Влияет на порядок
    /// </summary>
    PercentOfBase = 1,

    /// <summary>
    /// Фиксированная надбавка/скидка: коэф прибавляется к цене.
    /// Влияет на порядок
    /// </summary>
    Additive = 2, 
}