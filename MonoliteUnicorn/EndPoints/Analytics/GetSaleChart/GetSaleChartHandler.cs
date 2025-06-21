using Core.Interface;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Analytics;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace MonoliteUnicorn.EndPoints.Analytics.GetSaleChart;

public record GetSaleChartQuery(DateTime StartDate, DateTime EndDate, int? CurrencyId) : IQuery<GetSaleChartResult>;
public record GetSaleChartResult(Dictionary<DateTime, List<ChartDto>> ChartsData);

public class GetSaleChartHandler(DContext context) : IQueryHandler<GetSaleChartQuery, GetSaleChartResult>
{
    public async Task<GetSaleChartResult> Handle(GetSaleChartQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Unspecified);
        var endDate = DateTime.SpecifyKind(request.EndDate.Date.AddDays(1), DateTimeKind.Unspecified);

        var result = new Dictionary<DateTime, List<ChartDto>>();

        var salesForPeriod = await context.SaleContents
            .AsNoTracking()
            .Include(x => x.Sale)
            .Where(x => x.Sale.SaleDatetime >= startDate && x.Sale.SaleDatetime < endDate)
            .ToListAsync(cancellationToken);

        var grouped = salesForPeriod
            .GroupBy(x => x.Sale.SaleDatetime.Date);

        var targetCurrencyId = request.CurrencyId;
        var targetCurrency = await context.Currencies.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == targetCurrencyId, cancellationToken);

        foreach (var group in grouped)
        {
            result[group.Key] = [];
            int count = 0;
            decimal totalUsd = 0;
            decimal minUsd = decimal.MaxValue;
            decimal maxUsd = decimal.MinValue;
            
            foreach (var sale in group)
            {
                var usdValue = PriceGenerator.ConvertToNeededCurrency(sale.TotalSum, sale.Sale.CurrencyId, Global.UsdId);
                totalUsd += usdValue;
                minUsd = Math.Min(minUsd, usdValue);
                maxUsd = Math.Max(maxUsd, usdValue);
                
                count++;
            }

            var usdModel = new ChartDto
            {
                TotalSum = Math.Round(totalUsd, 2),
                Average = Math.Round(totalUsd / count, 2),
                Minimum = Math.Round(minUsd, 2),
                Maximum = Math.Round(maxUsd, 2),
                CurrencyId = Global.UsdId,
            };
            result[group.Key].Add(usdModel);
            if (targetCurrency == null) continue;
            var currencyModel = new ChartDto
            {
                TotalSum = Math.Round(PriceGenerator.ConvertToNeededCurrency(usdModel.TotalSum ?? 0, Global.UsdId, targetCurrency.Id), 2),
                Average = Math.Round(PriceGenerator.ConvertToNeededCurrency(usdModel.Average ?? 0, Global.UsdId, targetCurrency.Id), 2),
                Minimum = Math.Round(PriceGenerator.ConvertToNeededCurrency(usdModel.Minimum ?? 0, Global.UsdId, targetCurrency.Id), 2),
                Maximum = Math.Round(PriceGenerator.ConvertToNeededCurrency(usdModel.Maximum ?? 0, Global.UsdId, targetCurrency.Id), 2),
                CurrencyId = targetCurrency.Id,
            };
            result[group.Key].Add(currencyModel);
        }

        return new GetSaleChartResult(result);
    }
}
