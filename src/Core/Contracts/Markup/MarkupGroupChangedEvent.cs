using Contracts.Interfaces;

namespace Contracts.Markup;

public record MarkupGroupChangedEvent(int GroupId) : IContract;