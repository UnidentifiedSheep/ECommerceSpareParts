namespace Main.Application.Models.Currency;

public record UpdateRatesResult(Dictionary<int, decimal> Changed, List<string> NotFound);