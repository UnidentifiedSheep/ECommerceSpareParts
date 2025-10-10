using Core.Interfaces;

namespace Contracts;

public record MarkupGroupChangedEvent(int GroupId) : IContract;