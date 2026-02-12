using Contracts.Models.Markup;

namespace Contracts.Markup;

public record MarkupGroupGeneratedEvent
{
    public List<MarkupRangeStat> MarkupRanges { get; init; } = null!;
    public int CurrencyId { get; init; }
}