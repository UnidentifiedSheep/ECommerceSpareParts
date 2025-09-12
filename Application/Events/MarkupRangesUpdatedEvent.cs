using MediatR;

namespace Application.Events;

public record MarkupRangesUpdatedEvent() : INotification;