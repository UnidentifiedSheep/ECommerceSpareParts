using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models.Logistics;
using Main.Application.Extensions;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public abstract class LogisticsPricingStrategyBase : ILogisticsPricingStrategy
{
    public abstract LogisticPricingType Type { get; }
    public abstract LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items);

    /// <summary>
    /// Выполняет итерацию по позициям логистики и рассчитывает стоимость каждой позиции.
    /// </summary>
    /// <param name="context">
    /// Контекст расчета логистики, включающий минимальную цену, валюту и другие настройки.
    /// </param>
    /// <param name="items">
    /// Список позиций для расчета.
    /// </param>
    /// <param name="calculateCost">
    /// Функция, которая рассчитывает стоимость для одной позиции.
    /// Возвращает стоимость позиции в базовой валюте.
    /// </param>
    /// <param name="requirements">Флаг показывающий какие данные нужны для расчета стоимости</param>
    /// <returns>
    /// <see cref="LogisticsCalcResult"/> - результат расчета
    /// </returns>
    /// <example>
    /// Пример использования:
    /// <code>
    /// var result = strategy.Iterate(context, items, input =>
    /// {
    ///     // Рассчитать стоимость на основе веса и объема
    ///     decimal pricePerKg = 1000m;
    ///     decimal pricePerM3 = 10m;
    ///     return Math.Max(input.Weight * pricePerKg, input.Area * pricePerM3);
    /// });
    /// </code>
    /// </example>
    protected LogisticsCalcResult Iterate(LogisticsContext context, IEnumerable<LogisticsItem> items, 
        Func<LogisticsChargeInput, decimal> calculateCost, LogisticsDataRequirements requirements)
    {
        var resultItems = new List<LogisticsCalcItemResult>();
        var result = new LogisticsCalcResult
        {
            PricingModel = Type,
            MinimalPrice = context.MinimumPrice ?? 0
        };
        
        decimal totalWeight = 0;
        decimal totalArea = 0;
        
        decimal accumulatedCost = 0;
        
        foreach (var item in items)
        {
            if (item.Quantity <= 0) throw new ArgumentException("Количество должно быть больше или равно 0.");
            var weightPerItem = item.Weight.ToKg(item.WeightUnit);
            var weight =  weightPerItem * item.Quantity;
            var area = item.AreaM3 * item.Quantity;
            
            var (skipped, reason) = ValidatePerItemData(weightPerItem, item.AreaM3, requirements);

            if (skipped)
            {
                resultItems.Add(new LogisticsCalcItemResult(item.Id, 0, item.Quantity, area, item.AreaM3, weight, 
                    weightPerItem, WeightUnit.Kilogram, skipped, reason));
                continue;
            }
            
            totalWeight += weight;
            totalArea += area;
            
            decimal cost = Math.Round(calculateCost(new LogisticsChargeInput(area, weight, item.Quantity)), 2);

            if (cost < 0) throw new ArgumentException("Цена должна быть больше или равна 0");
            
            accumulatedCost += cost;
            resultItems.Add(new LogisticsCalcItemResult(item.Id, cost, item.Quantity, area, item.AreaM3, weight, 
                weightPerItem, WeightUnit.Kilogram, skipped, reason));
        }

        result.TotalAreaM3 = totalArea;
        result.TotalWeight = totalWeight;
        result.WeightUnit = WeightUnit.Kilogram;
        result.TotalCost = accumulatedCost;
        result.Items = resultItems;
        
        return result;
    }

    private static (bool skipped, IEnumerable<string>? reason) ValidatePerItemData(decimal weight, decimal area, 
        LogisticsDataRequirements requirements)
    {
        if (weight < 0) throw new ArgumentException("Вес должно быть больше или равно 0.");
        if (area < 0) throw new ArgumentException("Площадь должно быть больше или равно 0.");
        
        var reasons = new List<string>(2);
        
        if (requirements.HasFlag(LogisticsDataRequirements.Weight) && weight == 0)
            reasons.Add("Вес должен быть больше 0");

        if (requirements.HasFlag(LogisticsDataRequirements.Size) && area == 0)
            reasons.Add("Площадь должна быть больше 0");

        return reasons.Count > 0
            ? (true, reasons)
            : (false, null);
    }
}