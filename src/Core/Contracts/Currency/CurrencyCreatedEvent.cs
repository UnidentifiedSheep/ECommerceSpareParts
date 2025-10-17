using Contracts.Interfaces;
namespace Contracts.Currency;

public record CurrencyCreatedEvent(Models.Currency.Currency Currency) : IContract;