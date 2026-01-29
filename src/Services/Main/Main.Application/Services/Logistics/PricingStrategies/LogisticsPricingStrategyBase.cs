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
        Func<LogisticsChargeInput, decimal> calculateCost)
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
            var (skipped, reason) = Validate(item);
            
            var weightPerItem = item.Weight.ToKg(item.WeightUnit);
            var weight =  weightPerItem * item.Quantity;
            var area = item.AreaM3 * item.Quantity;
            totalWeight += weight;
            totalArea += area;
            
            decimal cost = Math.Round(calculateCost(new LogisticsChargeInput(area, weight, item.Quantity)), 2);

            if (cost < 0) throw new ArgumentException("Cost must be greater or equal to 0");
            
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

    private static (bool skipped, string? reason) Validate(LogisticsItem item)
    {
        var reason = new List<string>(3);
        if (item.Quantity <= 0) reason.Add("Количество должно быть больше 0");
        if (item.Weight <= 0) reason.Add("Вес должен быть больше 0");
        if (item.AreaM3 <= 0) reason.Add("Площадь должна быть больше 0");
        return reason.Count > 0 
            ? (true, string.Join('\n', reason))
            : (false, null);
    }
}