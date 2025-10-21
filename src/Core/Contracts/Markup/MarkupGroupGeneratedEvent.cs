using Contracts.Interfaces;
using Contracts.Models.Markup;

namespace Contracts.Markup;

public record MarkupGroupGeneratedEvent(List<MarkupRangeStat> MarkupRanges, int CurrencyId) : IContract;