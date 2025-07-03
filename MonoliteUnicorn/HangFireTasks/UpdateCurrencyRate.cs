using Core.RabbitMq.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions;
using MonoliteUnicorn.HangFireTasks.Models;
using MonoliteUnicorn.PostGres.Main;
using Newtonsoft.Json.Linq;

namespace MonoliteUnicorn.HangFireTasks;

public class UpdateCurrencyRate
{
    private const string BaseCurrency = "USD";
    private const string ExchangerBaseUrl = "https://api.apilayer.com/exchangerates_data/";
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly DContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateCurrencyRate(IConfiguration config, DContext context, HttpClient httpClient, IPublishEndpoint publishEndpoint)
    {
        _apiKey = config.GetValue<string>("ExchangeApiKey")!;
        _httpClient = httpClient;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    
    public async Task Run()
    {
        var newValues = await GetNewValues();
        await UpdateDatabaseValues(newValues);
    }
    private async Task UpdateDatabaseValues(LatestCurrencyModel latestCurrency)
    {
        var usd = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "USD");
        if (usd != null)
        {
            var currencyToUsd = await _context.CurrencyToUsds.FirstOrDefaultAsync(x => x.CurrencyId == usd.Id);
            if (currencyToUsd == null)
                await _context.CurrencyToUsds.AddAsync(new CurrencyToUsd { CurrencyId = usd.Id , ToUsd = 1});
            else
                currencyToUsd.ToUsd = 1;
        }
        else
        {
            usd = new Currency
            {
                Code = "USD",
                CurrencySign = "$",
                Name = "Доллар США",
                ShortName = "Дол.",
            };
            await _context.Currencies.AddAsync(usd);
            await _context.SaveChangesAsync();
            await _context.CurrencyToUsds.AddAsync(new CurrencyToUsd { CurrencyId = usd.Id , ToUsd = 1});
            await _context.SaveChangesAsync();
        }
        foreach (var rate in latestCurrency.Rates)
        {
            var currency = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Code == rate.Key);
            if(currency == null) continue;
            var currencyToUsd = await _context.CurrencyToUsds.AsNoTracking().FirstOrDefaultAsync(x => x.CurrencyId == currency.Id);
            if (currencyToUsd == null)
                await _context.Database.ExecuteSqlAsync($"INSERT INTO currency_to_usd (currency_id, to_usd) values ({currency.Id}, {rate.Value});");
            else
                await _context.Database.ExecuteSqlAsync($"update currency_to_usd set to_usd = {rate.Value} where currency_id = {currency.Id};");
            var currencyHistory = new CurrencyHistory
            {
                CurrencyId = currency.Id,
                PrevValue = currencyToUsd?.ToUsd ?? 0,
                NewValue = rate.Value
            };
            await _context.CurrencyHistories.AddAsync(currencyHistory);
            await _context.SaveChangesAsync();
        }

        await _publishEndpoint.Publish(new CurrencyRateChangedEvent());
    }
    
    private async Task<LatestCurrencyModel> GetNewValues()
    {
        string path = "latest";
        var uri = new UriBuilder(ExchangerBaseUrl + path);
        string neededCurrencies = await GetNeededCurrencies();
        uri.Query = $"symbols={neededCurrencies}&base={BaseCurrency}";
        
        using var requestMessage = new HttpRequestMessage();
        requestMessage.Method = HttpMethod.Get;
        requestMessage.RequestUri = uri.Uri;
        requestMessage.Headers.Add("apikey", _apiKey);
    
        var response = await _httpClient.SendAsync(requestMessage);
        var jObjectString = await response.Content.ReadAsStringAsync();
        var jObject = JObject.Parse(jObjectString);
        if (jObject["success"]?.Value<bool>() == false) throw new InvalidResponseException($"Invalid response {jObjectString}");
        var currencyRates = jObject.ToObject<LatestCurrencyModel>() ?? throw new UnableDeserializeException("Unable to deserialize exchange rate data.");
        return currencyRates;
    }

    private async Task<string> GetNeededCurrencies(CancellationToken cancellationToken = default)
    {
        var currencies = await _context.Currencies.AsNoTracking()
            .Where(x => x.Code != "USD")
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);
        string result = "";
        currencies.ForEach(x => result += $"{x},");
        return result.TrimEnd(',');
    }
    
    
}