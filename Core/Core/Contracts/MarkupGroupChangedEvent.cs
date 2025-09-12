using Core.Interfaces;

namespace Core.Contracts;

public record MarkupGroupChangedEvent(int GroupId) : IContract;