using Core.Interfaces;

namespace Contracts.Currency;

public record CurrencyCreatedEvent(Core.Models.Currency Currency) : IContract;