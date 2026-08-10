namespace Application.Common.Models;

public sealed record IntegrationEventEnvelope(
    object Message,
    string? RoutingKey);
