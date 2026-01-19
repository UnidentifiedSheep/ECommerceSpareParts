namespace Main.Enums;

public enum LogisticPricingType
{
    /// <summary>
    /// Без оплаты
    /// </summary>
    None,
    /// <summary>
    /// Оплата за заказ
    /// </summary>
    PerOrder,
    /// <summary>
    /// Оплата за площадь метры^3
    /// </summary>
    PerArea,
    /// <summary>
    /// За вес
    /// </summary>
    PerWeight,
    /// <summary>
    /// За вес и площадь
    /// </summary>
    PerAreaAndWeight
}