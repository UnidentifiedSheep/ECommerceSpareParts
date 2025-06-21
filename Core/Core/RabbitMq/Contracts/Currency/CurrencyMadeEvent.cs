namespace Core.RabbitMq.Contracts.Currency;

public record CurrencyMadeEvent(int Id, string ShortName, string Name, string CurrencySign, string Code);