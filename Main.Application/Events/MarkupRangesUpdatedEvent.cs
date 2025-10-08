using MediatR;

namespace Main.Application.Events;

public record MarkupRangesUpdatedEvent : INotification;